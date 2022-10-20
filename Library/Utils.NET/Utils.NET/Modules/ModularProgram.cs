using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Utils.NET.Logging;

namespace Utils.NET.Modules
{
    public class ModularProgram
    {
        public static void Command(string command)
        {
            instance.ProcessInput(command);
        }

        /// <summary>
        /// The static instance of the program
        /// </summary>
        public static ModularProgram instance = new ModularProgram();

        public static Manifest manifest = new Manifest();
        
        /// <summary>
        /// Runs the program with given modules
        /// </summary>
        public static void Run(params Module[] modules)
        {
            Log.Write("Loaded manifest file");
            Log.Write("Press Q to stop");
            Log.SetGlobalLog(new Log(Console.Write, Environment.CurrentManagedThreadId));

            for (int i = 0; i < modules.Length; i++)
                instance.AddModule(modules[i]);

            instance.LoadExternalModules();

            Log.Run();

            instance.Stop();
        }

        /// <summary>
        /// Stops the execution of the program
        /// </summary>
        public static void Exit()
        {
            Log.Stop();
        }

        /// <summary>
        /// Collection of all loaded modules
        /// </summary>
        private List<Module> Modules = new List<Module>();

        /// <summary>
        /// Adds a module to the program
        /// </summary>
        /// <param name="module"></param>
        public void AddModule(Module module)
        {
            lock (Modules)
            {
                Modules.Add(module);
                Log.Write("Starting Module: " + module.Name);
                module.Start();
            }
        }

        public T GetModule<T>()
        {
            lock (Modules)
            {
                foreach (var module in Modules)
                {
                    if (module is T t)
                        return t;
                }
                return default;
            }
        }

        /// <summary>
        /// Loads any modules in external dll's within the modules directory
        /// </summary>
        private void LoadExternalModules()
        {
            string path = "Modules";
            if (!Directory.Exists(path)) return;

            foreach (var file in Directory.EnumerateFiles(path))
            {
                if (!file.EndsWith(".dll", StringComparison.Ordinal)) continue; // only load dlls
                LoadModuleDll(file);
            }
        }

        /// <summary>
        /// Loads a module type from a given DLL, then adds the module to the program
        /// </summary>
        /// <param name="path"></param>
        private void LoadModuleDll(string path)
        {
            var assembly = Assembly.LoadFrom(path);
            var moduleType = typeof(Module);
            foreach (var type in assembly.GetTypes())
            {
                if (!type.IsSubclassOf(moduleType)) continue; // exlude all types that aren't modules
                AddModule((Module)Activator.CreateInstance(type));
            }
        }

        /// <summary>
        /// Stops the program and all modules
        /// </summary>
        private void Stop()
        {
            lock (Modules)
            {
                for (int i = 0; i < Modules.Count; i++)
                {
                    var module = Modules[i];

                    Log.Write("Stopping Module: " + module.Name);
                    module.Stop();
                }
            }
        }

        /// <summary>
        /// Processes input received from the console
        /// </summary>
        /// <param name="command"></param>
        public void ProcessInput(string input)
        {
            var split = input.Split(' ');
            if (split.Length == 0) return;
            Module module = null;
            if (Modules.Count > 1)
            {
                if (split.Length == 1)
                {
                    Log.Error("Single commands are not allowed when multiple modules are loaded");
                    return;
                }

                var name = split[0];
                foreach (var m in Modules)
                    if (m.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        module = m;
                        break;
                    }

                if (module == null)
                {
                    Log.Error($"Module '{name}' not found");
                    return;
                }

                var commands = new string[split.Length - 1];
                Array.Copy(split, 1, commands, 0, commands.Length);
                split = commands;
            }
            else if (Modules.Count == 0)
                return;
            else
                module = Modules[0];

            var args = new string[split.Length - 1];
            Array.Copy(split, 1, args, 0, args.Length);

            module.OnCommand(split[0], args);
        }
    }
}
