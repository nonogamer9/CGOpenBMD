using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.Purchasing
{
	public static class LocaleExtensions
	{
		public static readonly string XiaomiMiGamePay = "Xiaomi Mi Game Pay";

		private static readonly string[] Labels = new string[32]
		{
			"Chinese (Traditional)", "Czech", "Danish", "Dutch", "English (U.S.)", "French", "Finnish", "German", "Hebrew", "Hindi",
			"Italian", "Japanese", "Korean", "Norwegian", "Polish", "Portuguese (Portugal)", "Russian", "Spanish (Spain)", "Swedish", "Chinese (Simplified)",
			"English (Australia)", "English (Canada)", "English (U.K.)", "French (Canada)", "Greek", "Indonesian", "Malay", "Portuguese (Brazil)", "Spanish (Mexico)", "Thai",
			"Turkish", "Vietnamese"
		};

		private static readonly TranslationLocale[] GoogleLocales = new TranslationLocale[19]
		{
			TranslationLocale.zh_TW,
			TranslationLocale.cs_CZ,
			TranslationLocale.da_DK,
			TranslationLocale.nl_NL,
			TranslationLocale.en_US,
			TranslationLocale.fr_FR,
			TranslationLocale.fi_FI,
			TranslationLocale.de_DE,
			TranslationLocale.iw_IL,
			TranslationLocale.hi_IN,
			TranslationLocale.it_IT,
			TranslationLocale.ja_JP,
			TranslationLocale.ko_KR,
			TranslationLocale.no_NO,
			TranslationLocale.pl_PL,
			TranslationLocale.pt_PT,
			TranslationLocale.ru_RU,
			TranslationLocale.es_ES,
			TranslationLocale.sv_SE
		};

		private static readonly TranslationLocale[] AppleLocales = new TranslationLocale[28]
		{
			TranslationLocale.zh_CN,
			TranslationLocale.zh_TW,
			TranslationLocale.da_DK,
			TranslationLocale.nl_NL,
			TranslationLocale.en_AU,
			TranslationLocale.en_CA,
			TranslationLocale.en_GB,
			TranslationLocale.en_US,
			TranslationLocale.fi_FI,
			TranslationLocale.fr_FR,
			TranslationLocale.fr_CA,
			TranslationLocale.de_DE,
			TranslationLocale.el_GR,
			TranslationLocale.id_ID,
			TranslationLocale.it_IT,
			TranslationLocale.ja_JP,
			TranslationLocale.ko_KR,
			TranslationLocale.ms_MY,
			TranslationLocale.no_NO,
			TranslationLocale.pt_BR,
			TranslationLocale.pt_PT,
			TranslationLocale.ru_RU,
			TranslationLocale.es_MX,
			TranslationLocale.es_ES,
			TranslationLocale.sv_SE,
			TranslationLocale.th_TH,
			TranslationLocale.tr_TR,
			TranslationLocale.vi_VN
		};

		private static readonly TranslationLocale[] XiaomiLocales = new TranslationLocale[1] { TranslationLocale.zh_CN };

		private static string[] LabelsWithSupportedPlatforms;

		public static string[] GetLabelsWithSupportedPlatforms()
		{
			if (LabelsWithSupportedPlatforms != null)
			{
				return LabelsWithSupportedPlatforms;
			}
			LabelsWithSupportedPlatforms = new string[Enum.GetValues(typeof(TranslationLocale)).Length];
			List<TranslationLocale> list = GoogleLocales.ToList();
			List<TranslationLocale> list2 = AppleLocales.ToList();
			List<TranslationLocale> list3 = XiaomiLocales.ToList();
			int num = 0;
			foreach (TranslationLocale value in Enum.GetValues(typeof(TranslationLocale)))
			{
				List<string> list4 = new List<string>();
				if (list.Contains(value))
				{
					list4.Add("Google Play");
				}
				if (list2.Contains(value))
				{
					list4.Add("Apple");
				}
				if (list3.Contains(value))
				{
					list4.Add(XiaomiMiGamePay);
				}
				string text = string.Join(", ", list4.ToArray());
				LabelsWithSupportedPlatforms[num] = Labels[num] + " (" + text + ")";
				num++;
			}
			return LabelsWithSupportedPlatforms;
		}

		public static bool SupportedOnApple(this TranslationLocale locale)
		{
			return AppleLocales.Contains(locale);
		}

		public static bool SupportedOnGoogle(this TranslationLocale locale)
		{
			return GoogleLocales.Contains(locale);
		}
	}
}
