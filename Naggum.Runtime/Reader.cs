﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Naggum.Runtime
{
    public class Reader
    {
        /// <summary>
        /// Checks if the character is constituent, i.e. not whitespace or list separator.
        /// </summary>
        /// <param name="c">character to be checked</param>
        /// <returns>true if the character is constituent, false otherwise</returns>
        public static bool isConstituent(char c)
        {
            return (!Char.IsWhiteSpace(c))
                && c != '('
                && c != ')';
        }

        /// <summary>
        /// Reads a symbol from a stream.
        /// </summary>
        /// <param name="stream">stream to read from</param>
        /// <returns></returns>
        private static Object ReadSymbol(StreamReader reader)
        {
            bool in_symbol = true;
            StringBuilder symbol_name = new StringBuilder();
            while (in_symbol)
            {
                var ch = reader.Peek();
                if (ch < 0) throw new IOException("Unexpected end of stream.");
                if (isConstituent((char)ch))
                {
                    symbol_name.Append((char)reader.Read());
                }
                else
                {
                    in_symbol = false;
                }
            }
            if (symbol_name.Length > 0)
                return new Symbol(symbol_name.ToString());
            else
                throw new IOException("Empty symbol.");
        }

        /// <summary>
        /// Reads a list from input stream.
        /// </summary>
        /// <param name="stream">stream to read from</param>
        /// <returns></returns>
        private static Object ReadList(StreamReader reader)
        {
            bool in_list = true;
            Stack<Object> list_stack = new Stack<object>();
            Cons list = null;
            while (in_list)
            {
                var ch = reader.Peek();
                if (ch < 0) throw new IOException("Unexpected end of stream.");
                if ((char)ch != ')')
                {
                    list_stack.Push(ReadObject(reader));
                }
                else
                {
                    reader.Read(); //consume closing paren
                    in_list = false;
                }
            }
            while (list_stack.Count > 0) list = new Cons(list_stack.Pop(), list);
            return list;
        }

        /// <summary>
        /// Reads a string from input stream
        /// </summary>
        /// <param name="stream">input stream</param>
        /// <returns>a string that was read</returns>
        private static string ReadString(StreamReader reader)
        {
            bool in_string = true;
            bool single_escape = false;
            StringBuilder sbld = new StringBuilder();

            while (in_string)
            {
                var ch = reader.Read();
                if (single_escape)
                {
                    single_escape = false;
                    switch (ch)
                    {
                        case 'n': sbld.Append('\n'); break;
                        case 'r': sbld.Append('\r'); break;
                        case '\"': sbld.Append('"'); break;
                        case 't': sbld.Append('\t'); break;
                        case '\\': sbld.Append('\\'); break;
                        default: throw new Exception("Unknown escape sequence: \\" + ch);
                    }
                }
                else
                {
                    switch (ch)
                    {
                        case '\"': in_string = false; break;
                        case '\\': single_escape = true; break;
                        default: sbld.Append((char)ch); break;
                    }
                }

            }
            return sbld.ToString();
        }

        private static Object ReadObject(StreamReader reader)
        {
            while (Char.IsWhiteSpace((char)reader.Peek())) reader.Read(); //consume all leading whitespace
            var ch = reader.Peek();
            if (ch < 0) return null;
            if (ch == '(') //beginning of a list
            {
                reader.Read(); //consume opening list delimiter.
                return ReadList(reader);
            }
            if (ch == '\"') //beginning of a string
            {
                reader.Read(); //consume opening quote
                return ReadString(reader);
            }
            if (isConstituent((char)ch))
                return ReadSymbol(reader);

            throw new IOException("Unexpected char: " + (char)ch);
        }

        /// <summary>
        /// Reads an object from input stream.
        /// </summary>
        /// <param name="stream">stream to read from</param>
        /// <returns></returns>
        public static Object Read(Stream stream)
        {
            StreamReader reader = new StreamReader(stream);
            var obj = ReadObject(reader);
            return obj;
        }
    }
}
