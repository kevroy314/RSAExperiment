using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RSAExperiment
{
    class Program
    {
        //Shitty random number generator
        static Random randGen = new Random();

        //Test it!
        static void Main(string[] args)
        {
            for (int i = 0; i < 100000; i++)
            {
                int p = getPrime(0);
                int q = getPrime(1);
                int n = p * q;
                int eu = (p - 1) * (q - 1);
                int e = getRandomCoprime(eu);
                if (e == -1) Console.WriteLine("Failed to Generate Coprime");
                int d0 = eGCD(eu, e);
                int d = eu + d0;
                string testM = "Kevin is awesome!";
                List<int> encryptedM = encrypt(testM, e, n);
                string decryptedM = decrypt(encryptedM, d, n);
                if (testM != decryptedM) Console.WriteLine("Process Failed");
            }
            Console.WriteLine("End");
            Console.ReadLine();
        }

        //Decrypt the message using the private key (from http://en.wikipedia.org/wiki/RSA_(algorithm)#Decryption )
        static string decrypt(List<int> message, int d, int n)
        {
            string decryptedMessage = "";
            for (int i = 0; i < message.Count; i++)
                decryptedMessage += (char)(modular_pow(message[i],d,n));
            return decryptedMessage;
        }

        //Encrypt the message using the public key (from http://en.wikipedia.org/wiki/RSA_(algorithm)#Encryption )
        static List<int> encrypt(string message, int e, int n)
        {
            List<int> encryptedMessage = new List<int>();
            for (int i = 0; i < message.Length; i++)
            {
                int m = (int)(message[i]);
                int c = modular_pow(m, e, n);
                encryptedMessage.Add((int)c);
            }
            return encryptedMessage;
        }

        //From http://en.wikipedia.org/wiki/Modular_exponentiation#Memory-efficient_method
        static int modular_pow(int b, int e, int m)
        {
            int c = 1;
            for (int e_prime = 1; e_prime <= e; e_prime++)
                c = (c * b) % m;
            return c;
        }

        //From https://en.wikipedia.org/wiki/Euclidean_algorithm#Implementations
        static int GCD(int a0, int b0)
        {
            int a = a0;
            int b = b0;

            while(b != 0)
            {
                int t = b;
                b = a % t;
                a = t;
            }
            return a;
        }

        //From http://en.wikipedia.org/wiki/Extended_Euclidean_algorithm#Iterative_method_2
        static int eGCD(int a0, int b0)
        {
            int a = a0;
            int b = b0;
            int x = 0, lastx = 1, y = 1, lasty = 0;
            while(b != 0)
            {
                int quotient = (int)Math.Floor(((double)a / (double)b));
                int b1 = b;
                int a1 = a;
                b = (int)a1 % (int)b1;
                a = b1;
                int x1 = x;
                x = lastx - quotient*x1;
                lastx = x1;
                int y1 = y;
                y = lasty - quotient*y1;
                lasty = y1;
            }
            return lasty;
        }

        //Made this up based on definition of coprimes ( http://en.wikipedia.org/wiki/Coprime_integers ) in an attempt to generate a random coprime with as little bias as possible
        static int getRandomCoprime(int x)
        {
            //Start searching for a coprime from a random number within the limits of the RSA definition ( http://en.wikipedia.org/wiki/RSA_(algorithm)#Key_generation )
            int startSearchNum = randGen.Next(1,x-1);
            //Search in both directions
            int n0 = startSearchNum-1, n1 = startSearchNum+1;
            for (int i = 2; i < x; i++)
            {
                //If the value in either direction is within range and is coprime, flag it as valid
                bool n0_valid = false, n1_valid = false;
                if (!(n0>=x)&&!(n0<=1)&&GCD(n0, x) == 1)
                    n0_valid = true;
                if (!(n1>=x)&&!(n1<=1)&&GCD(n1, x) == 1)
                    n1_valid = true;
                if (n0_valid && n1_valid)
                {
                    //If both are valid at once, pick a random one to return
                    if (randGen.Next(0, 1) == 0)
                        return n0;
                    else
                        return n1;
                }
                //If only one is valid, return it
                else if (n0_valid)
                    return n0;
                else if (n1_valid)
                    return n1;

                //Iterate to the next search values in both directions
                n0 = startSearchNum - i;
                n1 = startSearchNum + i;
            }
            return -1; //Uh oh! We didn't find a coprime! Encryption will not work.
        }

        //Get a random prime
        static int getPrime(int x)
        {
            if (x == 0) return 61;
            else return 53;
        }
    }
}
