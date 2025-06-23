
using System.Security.Cryptography;
using System.Text;

namespace GCloudShared.Shared
{
    public static class AESCrypt
    {

        //public static string EncryptString(string key, string plainText)
        //{
        //    byte[] iv = new byte[16];
        //    using (Aes aes = Aes.Create())
        //    {
        //        aes.Key = Encoding.UTF8.GetBytes(key);
        //        aes.GenerateIV(); // Generate a random IV
        //        ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        //        using (MemoryStream memoryStream = new MemoryStream())
        //        {
        //            memoryStream.Write(aes.IV, 0, aes.IV.Length); // Write IV to the beginning of the stream
        //            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
        //            {
        //                using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
        //                {
        //                    streamWriter.Write(plainText);
        //                }
        //            }
        //            return Convert.ToBase64String(memoryStream.ToArray());
        //        }
        //    }
        //}

        //public static string DecryptString(string key, string cipherText)
        //{
        //    byte[] buffer = Convert.FromBase64String(cipherText);
        //    using (Aes aes = Aes.Create())
        //    {
        //        aes.Key = Encoding.UTF8.GetBytes(key);
        //        aes.IV = buffer.Take(16).ToArray(); // Extract IV from the first 16 bytes of the ciphertext
        //        ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        //        using (MemoryStream memoryStream = new MemoryStream(buffer.Skip(16).ToArray())) // Skip the IV
        //        {
        //            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
        //            {
        //                using (StreamReader streamReader = new StreamReader(cryptoStream))
        //                {
        //                    return streamReader.ReadToEnd();
        //                }
        //            }
        //        }
        //    }
        //}












        public static string EncryptString(string key, string plainText)
        {
            byte[] iv = new byte[16];
            byte[] array = new byte[0]; ;

            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = Encoding.UTF8.GetBytes(key);
                    aes.IV = iv;

                    ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                    using (MemoryStream memoryStream = new MemoryStream())
                    {

                        try
                        {

                            using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                            {

                                try
                                {
                                    using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                                    {
                                        //streamWriter.Write(plainText);
                                        streamWriter.AutoFlush = true;
                                        try
                                        {
                                            streamWriter.Write(plainText);

                                            streamWriter.Flush();
                                            streamWriter.Close();
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine($"An error occurred: {ex.Message}");
                                        }

                                        //array = memoryStream.ToArray();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"An error occurred: {ex.Message}");
                                }


                                array = memoryStream.ToArray();
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"An error occurred: {ex.Message}");
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }



            return Convert.ToBase64String(array);
        }



        //public static string EncryptString(string key, string plainText)
        //{
        //    byte[] iv = new byte[16];
        //    byte[] array = new byte[0]; // Initialize to empty array

        //    using (Aes aes = Aes.Create())
        //    {
        //        aes.Key = Encoding.UTF8.GetBytes(key);
        //        aes.IV = iv;

        //        ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

        //        using (MemoryStream memoryStream = new MemoryStream())
        //        {
        //            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
        //            {
        //                using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
        //                {
        //                    streamWriter.Write(plainText);
        //                    streamWriter.Flush();
        //                }

        //                array = memoryStream.ToArray();
        //            }
        //        }
        //    }

        //    return Convert.ToBase64String(array);
        //}

        //public static string EncryptString(string b_key, string plainText)
        //{
        //    byte[] array;

        //    using (Aes aes = Aes.Create())
        //    {
        //        aes.Key = Convert.FromBase64String(b_key);

        //        ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

        //        using (MemoryStream memoryStream = new MemoryStream())
        //        {
        //            // Adding aes.IV to the stream's start.
        //            memoryStream.Write(aes.IV);
        //            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
        //            {
        //                using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
        //                {
        //                    streamWriter.Write(plainText);
        //                }
        //            }
        //            array = memoryStream.ToArray();
        //        }
        //    }

        //    // The final encrypted outcome will be aes.IV+encryptedtext.
        //    return Convert.ToBase64String(array);
        //}

        public static string DecryptString(string key, string cipherText)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }


        //public static string EncryptString(string keyHex, string plainText)
        //{
        //    byte[] iv = new byte[16];
        //    byte[] array;

        //    using (Aes aes = Aes.Create())
        //    {
        //        aes.Key = Convert.FromHexString(keyHex); // Convert hex string to byte array
        //        aes.IV = iv;

        //        ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

        //        using (MemoryStream memoryStream = new MemoryStream())
        //        {
        //            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
        //            {
        //                using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
        //                {
        //                    streamWriter.Write(plainText);
        //                }

        //                array = memoryStream.ToArray();
        //            }
        //        }
        //    }

        //    return Convert.ToBase64String(array);
        //}

        //public static string DecryptString(string keyHex, string cipherText)
        //{
        //    byte[] iv = new byte[16];
        //    byte[] buffer = Convert.FromBase64String(cipherText);

        //    using (Aes aes = Aes.Create())
        //    {
        //        aes.Key = Convert.FromHexString(keyHex); // Convert hex string to byte array
        //        aes.IV = iv;
        //        ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

        //        using (MemoryStream memoryStream = new MemoryStream(buffer))
        //        {
        //            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
        //            {
        //                using (StreamReader streamReader = new StreamReader(cryptoStream))
        //                {
        //                    return streamReader.ReadToEnd();
        //                }
        //            }
        //        }
        //    }
        //}
    }
}
