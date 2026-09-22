using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace UnityEngine.Purchasing
{
	[Serializable]
	public class LocalizedProductDescription
	{
		public TranslationLocale googleLocale = TranslationLocale.en_US;

		[SerializeField]
		private string title;

		[SerializeField]
		private string description;

		public string Title
		{
			get
			{
				return DecodeNonLatinCharacters(title);
			}
			set
			{
				title = EncodeNonLatinCharacters(value);
			}
		}

		public string Description
		{
			get
			{
				return DecodeNonLatinCharacters(description);
			}
			set
			{
				description = EncodeNonLatinCharacters(value);
			}
		}

		public LocalizedProductDescription Clone()
		{
			LocalizedProductDescription localizedProductDescription = new LocalizedProductDescription();
			localizedProductDescription.googleLocale = googleLocale;
			localizedProductDescription.Title = Title;
			localizedProductDescription.Description = Description;
			return localizedProductDescription;
		}

		private static string EncodeNonLatinCharacters(string s)
		{
			if (s == null)
			{
				return s;
			}
			StringBuilder stringBuilder = new StringBuilder();
			foreach (char c in s)
			{
				if (c > '\u007f')
				{
					int num = c;
					string value = "\\u" + num.ToString("x4");
					stringBuilder.Append(value);
				}
				else
				{
					stringBuilder.Append(c);
				}
			}
			return stringBuilder.ToString();
		}

		private static string DecodeNonLatinCharacters(string s)
		{
			if (s == null)
			{
				return s;
			}
			return Regex.Replace(s, "\\\\u(?<Value>[a-zA-Z0-9]{4})", (Match m) => ((char)int.Parse(m.Groups["Value"].Value, NumberStyles.HexNumber)).ToString());
		}
	}
}
