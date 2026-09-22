using System.IO;
using System.Net;
using System.Text;

namespace UnityEngine.Purchasing
{
	internal class MoolahUtil
	{
		public static string GetResponseString(HttpWebResponse webresponse)
		{
			if (webresponse == null)
			{
				return null;
			}
			using (Stream stream = webresponse.GetResponseStream())
			{
				StreamReader streamReader = new StreamReader(stream, Encoding.UTF8);
				return streamReader.ReadToEnd();
			}
		}
	}
}
