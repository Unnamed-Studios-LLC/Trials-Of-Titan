using ServerNode.Net.Packets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utils.NET.Logging;
using Utils.NET.Modules;
using Utils.NET.Net.Tcp;
using Utils.NET.Utils;

namespace ServerNode.Net
{
    public enum ProgramState
    {
        NotStarted,
        Starting,
        Running,
        Updating,
        Restarting,
        Done
    }

    public class ProgramListener : NetListener<ProgramConnection, NPacket>
    {
        private class ProgramInfo
        {
            public ProgramConnection connection;

            public ProgramState state = ProgramState.NotStarted;

            public DateTime startTime = DateTime.Now;

            public ProgramListener listener;

            public bool runOnce;

            public int programType;

            public void Exited(object sender, EventArgs args)
            {
                if (runOnce)
                {
                    state = ProgramState.Done;
                }
                else if (state == ProgramState.Running || state == ProgramState.Starting)
                {
                    state = ProgramState.NotStarted;
                    Log.Write("Program exited, type: " + programType);
                }
            }
        }

        private ConcurrentDictionary<int, ProgramInfo> programs = new ConcurrentDictionary<int, ProgramInfo>();

        private Timer timer;

        public ProgramListener() : base(ProgramConnection.Port)
        {
        }

        public override void Start()
        {
            base.Start();

            timer = new Timer(TimerElapsed, null, 0, 5000);
        }

        public override void Stop()
        {
            base.Stop();

            timer.Dispose();
        }

        private void TimerElapsed(object state)
        {
            StartPrograms();
        }

        private void StartPrograms()
        {
            foreach (var directory in Directory.EnumerateDirectories(NodeListener.Program_Package_Directory))
            {
                var info = new DirectoryInfo(directory);
                if (info.Attributes.HasFlag(FileAttributes.Hidden)) continue;

                foreach (var manifestPath in Directory.EnumerateFiles(directory, "*.mfst", SearchOption.TopDirectoryOnly))
                {
                    var manifest = new Manifest(manifestPath);
                    var programType = manifest.Value("programType", -1);
                    if (programType == -1)
                    {
                        Log.Write("Failed to read program type from manifest");
                        break;
                    }

                    ProgramInfo programInfo;
                    if (programs.TryGetValue(programType, out programInfo))
                    {
                        if (programInfo.state == ProgramState.NotStarted)
                        {
                            programInfo.state = ProgramState.Starting;
                            StartProgram(directory, programInfo);
                        }
                    }
                    else
                    {
                        programInfo = new ProgramInfo()
                        {
                            connection = null,
                            state = ProgramState.Starting,
                            listener = this,
                            runOnce = manifest.Value<bool>("runOnce", false),
                            programType = programType
                        };
                        if (!programs.TryAdd(programType, programInfo)) break;
                        StartProgram(directory, programInfo);
                    }
                    break;
                }
            }
        }

        protected override void HandleConnection(ProgramConnection connection)
        {
            connection.onVerify += ConnectionVerified;
            connection.ReadAsync();
        }

        public void ConnectionVerified(ProgramConnection connection)
        {
            if (!programs.TryGetValue(connection.programType, out var programInfo))
            {
                connection.Disconnect();
                return;
            }

            if (programInfo.connection != null)
            {
                programInfo.connection.Disconnect();
            }

            programInfo.connection = connection;
            programInfo.state = ProgramState.Running;

            Log.Write("Program Verified, type: " + connection.programType);
        }

        public void StartProgram(int programType)
        {
            var directory = NodeListener.Program_Package_Directory + "Program-" + programType;
            if (!Directory.Exists(directory)) return;
            if (!programs.TryGetValue(programType, out var info)) return;
            info.state = ProgramState.Starting;
            StartProgram(directory, info);
        }

        private void StartProgram(string programDirectoryPath, ProgramInfo info)
        {
            foreach (var executable in Directory.EnumerateFiles(programDirectoryPath, "*.exe", SearchOption.TopDirectoryOnly))
            {
                var process = new Process();
                process.StartInfo.FileName = Path.GetFileName(executable);
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.WorkingDirectory = programDirectoryPath;
                process.Start();

                process.EnableRaisingEvents = true;
                process.Exited += info.Exited;
                return;
            }
        }

        public void EndProgram(int programType, ProgramState state)
        {
            if (!programs.TryRemove(programType, out var info)) return;
            EndProgram(info, state);
        }

        private void EndProgram(ProgramInfo info, ProgramState state)
        {
            info.state = state;
            info.connection.Disconnect();
            if (info.connection.processId > 0)
                ProcessUtils.WaitForProcessToClose(info.connection.processId, 5, true);
        }
    }
}
