using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;

namespace ItemsReader
{
    class CParser
    {
        string document;
        string filename;
        byte xor;


        public CParser(string filename)
        {
            this.filename = filename;
        }

        public bool Load()
        {
            if (!File.Exists(this.filename))
                return false;

            this.document = File.ReadAllText(filename);

            return true;
        }

        public string Find(string start,string end)
        {
            string pattern = String.Format("{0}(.*?){1}", start, end);
            return Regex.Match(this.document, pattern,RegexOptions.Singleline).Groups[1].Value;
        }

        public string Find(string input,string start,string end)
        {
            string pattern = String.Format("{0}(.*?){1}", start, end);
            return Regex.Match(input, pattern, RegexOptions.Singleline).Groups[1].Value;
        }

        public MatchCollection FindCollection(string input,string start,string end)
        {
            string pattern = String.Format("{0}(.*?){1}", start, end);
            return Regex.Matches(input, pattern ,RegexOptions.Singleline);
        }

        public string ToHex(string input)
        {
            StringBuilder builder = new StringBuilder(input.Length * 2);
            foreach (byte num2 in Encoding.Default.GetBytes(input))
            {
                builder.AppendFormat("{0:X2}", num2);
            }
            return builder.ToString();
        }

        public string FromHex(string input)
        {
            var bytes = new byte[input.Length / 2];
            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(input.Substring(i * 2, 2), 16);
            }

            return Encoding.Default.GetString(bytes);
        }

        public void Crypt(ref byte[] input,byte xor)
        {
            for(int i=0;i<input.Length;i++)
            {
                input[i] ^= xor;
            }
        }

        public int GetKey(string input)
        {
            return ((int)input[0]^(int)'<');
        }

        public void Decrypt()
        {
            string file = FromHex(this.document);
            xor = (byte)GetKey(file.Substring(0,10));
            byte[] file_decrypt=Encoding.Default.GetBytes(file);
            Crypt(ref file_decrypt, xor);
            this.document = Encoding.Default.GetString(file_decrypt);
        }

        private string Encrypt(string input)
        {
            byte[] temp = Encoding.Default.GetBytes(input);
            Crypt(ref temp, xor);
            return ToHex(Encoding.Default.GetString(temp));
        }

        public void Save(string text)
        {
            File.WriteAllText(filename, Encrypt(text));
        }

        public string GetDocument
        {
            get { return this.document; }
        }
    }
}
