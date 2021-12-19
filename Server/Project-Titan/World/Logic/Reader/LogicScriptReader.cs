using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Utils.NET.Geometry;
using Utils.NET.Logging;
using World.Logic.Actions;

namespace World.Logic.Reader
{
    public class LogicScriptReader
    {
        private class LogicScriptContext
        {
            /// <summary>
            /// The current line being read
            /// </summary>
            public int line;

            /// <summary>
            /// The current character being read within a line
            /// </summary>
            public int character;

            public override string ToString()
            {
                return $"({line}, {character})";
            }
        }

        /// <summary>
        /// The underlying stream being read
        /// </summary>
        private Stream stream;

        /// <summary>
        /// The reader of the stream
        /// </summary>
        private StreamReader reader;

        /// <summary>
        /// The context of the reader
        /// </summary>
        private LogicScriptContext context;

        public LogicScriptReader(Stream stream)
        {
            this.stream = stream;
            reader = new StreamReader(stream);

            context = new LogicScriptContext();
        }

        public LogicScriptReader(StreamReader reader)
        {
            stream = reader.BaseStream;
            this.reader = reader;

            context = new LogicScriptContext();
        }

        public List<T> ReadActions<T>() where T : ReadableAction
        {
            return ReadActions().Where(_ => _ is T).Select(_ => (T)_).ToList();
        }

        public List<ReadableAction> ReadActions()
        {
            var actions = new List<ReadableAction>();
            while (true)
            {
                var c = (char)reader.Peek();
                if (reader.EndOfStream) return actions;
                if (c == '}')
                {
                    Read();
                    return actions;
                }

                if (char.IsLetterOrDigit(c))
                {
                    actions.Add(ReadAction());
                }
                Read();
            }
        }

        public ReadableAction ReadAction()
        {
            var builder = new StringBuilder();
            while (true) // read action name
            {
                var c = (char)reader.Peek();
                if (!char.IsLetterOrDigit(c) && c != '_')
                {
                    if (c == '(')
                    {
                        Read();
                        break;
                    }
                    throw new InvalidDataException(context + " Unexpected character found while reading action name");
                }
                builder.Append(c);
                Read();
            }

            ReadableAction action = null;
            var name = builder.ToString().Trim();
            if (ReadableAction.actionTypes.TryGetValue(name, out var actionType))
            {
                action = (ReadableAction)Activator.CreateInstance(actionType);
            }
            else
            {
                throw new InvalidDataException(context + $" Action name of {name} does not exist");
            }

            builder.Clear();
            while (true) // read action parameters
            {
                var c = Read();
                if (char.IsLetterOrDigit(c) || c == ' ')
                {
                    builder.Append(c);
                }
                else if (c == ':')
                {
                    if (!action.ReadParameterValue(builder.ToString().Trim(), this))
                    {
                        // parameter not handled, parameter does not exists or is incorrectly spelled
                    }

                    builder.Clear();
                }
                else if (c == ')')
                {
                    break;
                }
            }

            return action;
        }

        public string ReadString()
        {
            var builder = new StringBuilder();
            bool foundQuote = false;
            while (true)
            {
                char c = Read();
                if (!foundQuote)
                {
                    foundQuote = c == '"';
                    if (!foundQuote && c != ' ')
                        throw new InvalidDataException(context + " Unexpected character found while reading string value");
                }
                else
                {
                    if (c == '\\')
                    {
                        c = Read();
                        switch (c)
                        {
                            case 'n':
                                c = '\n';
                                break;
                        }
                    }
                    else if (c == '"')
                    {
                        return builder.ToString();
                    }

                    builder.Append(c);
                }
            }
        }

        public int ReadInt()
        {
            var builder = new StringBuilder();
            bool negative = false;
            while (true)
            {
                char c = (char)reader.Peek();
                if (c == ' ')
                {
                    Read();
                }
                else if (!char.IsNumber(c) && c != '-')
                {
                    if (builder.Length == 0)
                        throw new InvalidDataException(context + " No integer available to read");

                    if (c != ',' && c != ')')
                        throw new InvalidDataException(context + " Unexpected character found while reading integer value");
                    return int.Parse(builder.ToString()) * (negative ? -1 : 1);
                }
                else
                {
                    builder.Append(Read());
                }
            }
        }

        public float ReadFloat()
        {
            var builder = new StringBuilder();
            while (true)
            {
                char c = (char)reader.Peek();
                if (c == ' ')
                {
                    Read();
                }
                else if (!char.IsNumber(c) && c != '.' && c != '-')
                {
                    if (builder.Length == 0)
                        throw new InvalidDataException(context + " No number available to read");

                    if (c != ',' && c != ')')
                        throw new InvalidDataException(context + " Unexpected character found while reading float value");
                    return float.Parse(builder.ToString());
                }
                else
                {
                    builder.Append(Read());
                }
            }
        }

        public float ReadAngle()
        {
            return ReadFloat() * AngleUtils.PI / 180f;
        }

        private string[] ReadArrayString()
        {
            var builder = new StringBuilder();
            bool foundBracket = false;
            var values = new List<string>();
            while (true)
            {
                if (!foundBracket)
                {
                    char c = Read();
                    foundBracket = c == '[';
                    if (!foundBracket && c != ' ')
                        throw new InvalidDataException(context + " Unexpected character found while reading array value");
                }
                else
                {
                    char c = (char)reader.Peek();
                    if (c != ' ')
                    {
                        if (c == '"')
                        {
                            builder.Append(ReadString());
                        }
                    }

                    c = Read();

                    if (c == ',')
                    {
                        values.Add(builder.ToString());
                        builder.Clear();
                    }
                    else if (c == ']')
                    {
                        values.Add(builder.ToString());
                        return values.ToArray();
                    }
                    else if (c != ' ')
                        builder.Append(c);
                }
            }
        }

        public string[] ReadStringArray()
        {
            return ReadArrayString();
        }

        public int[] ReadIntArray()
        {
            return ReadArrayString().Select(_ => int.Parse(_)).ToArray();
        }

        public float[] ReadFloatArray()
        {
            return ReadArrayString().Select(_ => float.Parse(_)).ToArray();
        }

        public bool ReadBool()
        {
            var boolStr = ReadString();
            return !(boolStr.Equals("no", StringComparison.OrdinalIgnoreCase) || boolStr.Equals("false", StringComparison.OrdinalIgnoreCase));
        }

        private char Read()
        {
            char c = (char)reader.Peek();
            if (c == '\n')
            {
                context.line++;
                context.character = 0;
            }
            else
            {
                context.character++;
            }
            reader.Read();
            return c;
        }

        private void ReadUntil(char character)
        {
            while (!reader.EndOfStream && Read() != character) { }
        }
    }
}
