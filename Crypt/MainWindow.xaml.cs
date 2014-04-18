using System;
using System.IO;
using System.Security.Cryptography;
using System.ComponentModel;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Drawing;
using System.Windows.Media.Imaging;

namespace Crypt
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //cry();
        }
        public void cry()
        {
            try
            {

                //string original = "d:\\wallpapers\\batman_arkham_origins_12-wallpaper-1366x768.jpg";
                byte[] imageArray = File.ReadAllBytes("d:\\0.png");

                string s = System.Text.Encoding.ASCII.GetString(imageArray, 0, imageArray.Length);
                s = System.BitConverter.ToString(imageArray);
                // Create a new instance of the RijndaelManaged 
                // class.  This generates a new key and initialization  
                // vector (IV). 
                using (RijndaelManaged myRijndael = new RijndaelManaged())
                {

                    myRijndael.GenerateKey();
                    myRijndael.GenerateIV();
                    // Encrypt the string to an array of bytes. 
                    byte[] encrypted = EncryptStringToBytes(s, myRijndael.Key, myRijndael.IV);

                    // Decrypt the bytes to a string. 
                    //string roundtrip = DecryptStringFromBytes(encrypted, myRijndael.Key, myRijndael.IV);
                    byte[] bytes = DecryptStringFromBytes(encrypted, myRijndael.Key, myRijndael.IV);
                    //Display the original data and the decrypted data.
                    i1.Source=new BitmapImage(new Uri(@"d:\\0.png"));

                    //byte[] bytes = new byte[roundtrip.Length];
                    //System.Buffer.BlockCopy(roundtrip.ToCharArray(), 0, bytes, 0, bytes.Length);
                    string loc = "d:\\file.jpg";
                    File.WriteAllBytes(loc, bytes);
                    
                    //s = System.Text.Encoding.ASCII.GetString(encrypted, 0, encrypted.Length);
                    //s = System.BitConverter.ToString(encrypted);
                    //l1.Text = roundtrip;
                    i2.Source = new BitmapImage(new Uri(@"d:\\file.jpg"));
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e.Message);
            }
        }
        static byte[] EncryptStringToBytes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments. 
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("Key");
            byte[] encrypted;
            // Create an RijndaelManaged object 
            // with the specified key and IV. 
            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                rijAlg.Key = Key;
                rijAlg.IV = IV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for encryption. 
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {

                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }


            // Return the encrypted bytes from the memory stream. 
            return encrypted;

        }

        static byte[] DecryptStringFromBytes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments. 
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("Key");

            // Declare the string used to hold 
            // the decrypted text. 
            string plaintext = null;

            // Create an RijndaelManaged object 
            byte[] decrypted;
            // with the specified key and IV. 
            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                rijAlg.Key = Key;
                rijAlg.IV = IV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for decryption. 
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream 
                            // and place them in a string. 
                            //plaintext = srDecrypt.ReadToEnd();
                        }
                        decrypted = msDecrypt.ToArray();
                    }
                }

            }

            return decrypted;

        }

        private void bt1_Click(object sender, RoutedEventArgs e)
        {
            cry();
        }

        private void bt2_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}