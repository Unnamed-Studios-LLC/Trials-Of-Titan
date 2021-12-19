using ServerNode.Net.Packets;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Utils.NET.Collections;
using Utils.NET.Logging;
using Utils.NET.Modules;
using Utils.NET.Net.Tcp;

namespace ServerNode.Net
{
    public class NodeListener : NetListener<NodeConnection, NPacket>
    {
        public const string Program_Package_Directory = "Programs/";

        public const string Program_Cache_Directory = "Programs/.cache/";

        public ProgramListener programListener;

        private NodeConnection connection;

        public NodeListener() : base(NodeConnection.Port)
        {
            SetupDirectories();
        }

        private void SetupDirectories()
        {
            if (!Directory.Exists(Program_Package_Directory))
                Directory.CreateDirectory(Program_Package_Directory);

            if (!Directory.Exists(Program_Cache_Directory))
            {
                var dir = Directory.CreateDirectory(Program_Cache_Directory);
                dir.Attributes |= FileAttributes.Hidden;
            }
        }

        protected override void HandleConnection(NodeConnection connection)
        {
            connection.handlers.Add(NPacketType.Update, ReceivedUpdate);
            connection.handlers.Add(NPacketType.VersionCheck, ReceivedVersionCheck);
            SetConnection(connection);
        }

        private void SetConnection(NodeConnection connection)
        {
            if (this.connection != null)
            {
                this.connection.Disconnect();
            }

            Log.Write("Received Connection from: " + connection.RemoteEndPoint.Address);
            this.connection = connection;
            connection.ReadAsync();
        }

        public void ReceivedUpdate(NPacket packet, NodeConnection connection)
        {
            Log.Write("Received Update");

            var update = (NUpdate)packet;
            programListener.EndProgram(update.programType, ProgramState.Updating);

            var programDirectoryPath = GetProgramDirectory(update.programType);
            var zipPath = GetProgramZipPath(update.programType);
            var checksumPath = GetProgramChecksumPath(update.programType);

            if (Directory.Exists(programDirectoryPath))
                Directory.Delete(programDirectoryPath, true);
            var dir = Directory.CreateDirectory(programDirectoryPath);
            dir.Attributes |= FileAttributes.Hidden;

            File.WriteAllBytes(zipPath, update.zip);
            File.WriteAllBytes(checksumPath, update.checksum);

            ZipFile.ExtractToDirectory(zipPath, programDirectoryPath);

            dir.Attributes &= ~FileAttributes.Hidden;

            connection.SendTokenResponse(new NUpdateResponse(true)
            {
                Token = update.Token
            });
        }

        public void ReceivedVersionCheck(NPacket packet, NodeConnection connection)
        {
            var versionCheck = (NVersionCheck)packet;
            bool needsUpdate = true;

            var checksum = LoadChecksum(versionCheck.programType);
            if (checksum != null)
            {
                needsUpdate = !checksum.SequenceEqual(versionCheck.checksum);
            }

            Log.Write("Received Version Check, needs update: " + needsUpdate);

            connection.SendTokenResponse(new NVersionCheckResponse(needsUpdate)
            {
                Token = versionCheck.Token
            });
        }

        private byte[] LoadChecksum(int type)
        {
            var path = GetProgramChecksumPath(type);
            if (!File.Exists(path)) return null;
            return File.ReadAllBytes(path);
        }

        private string GetProgramDirectory(int type)
        {
            return Program_Package_Directory + GetProgramName(type) + "/";
        }

        private string GetProgramName(int type)
        {
            return "Program-" + type;
        }

        private string GetProgramChecksumPath(int type)
        {
            return Program_Cache_Directory + GetProgramName(type) + ".md5";
        }

        private string GetProgramZipPath(int type)
        {
            return Program_Cache_Directory + GetProgramName(type) + ".zip";
        }
    }
}
