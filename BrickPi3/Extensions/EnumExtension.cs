//////////////////////////////////////////////////////////
// This code has been originally created by Laurent Ellerbach
// It intend to make the excellent BrickPi3 from Dexter Industries working
// on a RaspberryPi 2 or 3 runing Windows 10 IoT Core in Universal
// Windows Platform.
// Credits:
// - Dexter Industries Code
// - MonoBrick for great inspiration regarding sensors implementation in C#
//
// This code is origianlly created for the original BrickPi
// see https://github.com/ellerbach/BrickPi
//
// This code is under https://opensource.org/licenses/ms-pl
//
//////////////////////////////////////////////////////////

using System;

namespace BrickPi3.Extensions
{
	/// <summary>
	/// Extensions to get next or previous enum
	/// </summary>
	internal static class EnumExtensions
	{

	    public static T Next<T>(this T src) where T : struct
	    {
            //TODO
	        //if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argumnent {0} is not an Enum", typeof(T).FullName));
	
	        T[] Arr = (T[])Enum.GetValues(src.GetType());
	        int j = Array.IndexOf<T>(Arr, src) + 1;
	        return (Arr.Length==j) ? Arr[0] : Arr[j];            
	    }
	    
	    public static T Previous<T>(this T src) where T : struct
	    {
            //TODO
	        //if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argumnent {0} is not an Enum", typeof(T).FullName));
	
	        T[] Arr = (T[])Enum.GetValues(src.GetType());
	        int j = Array.IndexOf<T>(Arr, src) -1;
	        return (j < 0) ? Arr[Arr.Length-1] : Arr[j];            
	    }
	}
}

