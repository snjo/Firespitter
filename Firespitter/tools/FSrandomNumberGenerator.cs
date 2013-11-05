using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class FSrandomNumberGenerator
{
    public static Random rnd = new Random();
    public static int rndInt(int min, int max)
    {
        return rnd.Next(min, max);
    }
}
