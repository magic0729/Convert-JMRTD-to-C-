using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace org.jmrtd.lds.icao
{
	public class ICAOCountry
	{
		private static readonly List<ICAOCountry> VALUES;

		public static readonly ICAOCountry DE = new ICAOCountry("DE", "D<<", "Germany", "German");
		public static readonly ICAOCountry RKS = new ICAOCountry("KS", "RKS", "Republic of Kosovo", "Kosovar");
		public static readonly ICAOCountry GBD = new ICAOCountry("GB", "GBD", "British Dependent territories citizen");
		public static readonly ICAOCountry GBN = new ICAOCountry("GB", "GBN", "British National (Overseas)");
		public static readonly ICAOCountry GBO = new ICAOCountry("GB", "GBO", "British Overseas citizen");
		public static readonly ICAOCountry GBP = new ICAOCountry("GB", "GBP", "British Protected person");
		public static readonly ICAOCountry GBS = new ICAOCountry("GB", "GBS", "British Subject");
		public static readonly ICAOCountry XXA = new ICAOCountry("XX", "XXA", "Stateless person", "Stateless");
		public static readonly ICAOCountry XXB = new ICAOCountry("XX", "XXB", "Refugee", "Refugee");
		public static readonly ICAOCountry XXC = new ICAOCountry("XX", "XXC", "Refugee (other)", "Refugee (other)");
		public static readonly ICAOCountry XXX = new ICAOCountry("XX", "XXX", "Unspecified", "Unspecified");
		public static readonly ICAOCountry EUE = new ICAOCountry("EU", "EUE", "Europe", "European");
		public static readonly ICAOCountry UNO = new ICAOCountry("UN", "UNO", "United Nations Organization");
		public static readonly ICAOCountry UNA = new ICAOCountry("UN", "UNA", "United Nations Agency");
		public static readonly ICAOCountry UNK = new ICAOCountry("UN", "UNK", "United Nations Interim Administration Mission in Kosovo");
		public static readonly ICAOCountry XBA = new ICAOCountry("XX", "XBA", "African Development Bank (ADB)");
		public static readonly ICAOCountry XIM = new ICAOCountry("XX", "XIM", "African Export-Import Bank (AFREXIM bank)");
		public static readonly ICAOCountry XCC = new ICAOCountry("XC", "XCC", "Carribean Community or one of its emissaries (CARICOM)");
		public static readonly ICAOCountry XCE = new ICAOCountry("XX", "XCE", "Council of Europe");
		public static readonly ICAOCountry XCO = new ICAOCountry("XX", "XCO", "Common Market for Eastern an Southern Africa (COMESA)");
		public static readonly ICAOCountry XEC = new ICAOCountry("XX", "XEC", "Economic Community of West African States (ECOWAS)");
		public static readonly ICAOCountry XPO = new ICAOCountry("XP", "XPO", "International Criminal Police Organization (INTERPOL)");
		public static readonly ICAOCountry XES = new ICAOCountry("XX", "XES", "Organization of Eastern Caribbean States (OECS)");
		public static readonly ICAOCountry XMP = new ICAOCountry("XX", "XMP", "Parliamentary Assembly of the Mediterranean (PAM)");
		public static readonly ICAOCountry XOM = new ICAOCountry("XO", "XOM", "Sovereign Military Order of Malta or one of its emissaries");
		public static readonly ICAOCountry XDC = new ICAOCountry("XX", "XDC", "Southern African Development Community");

		static ICAOCountry()
		{
			VALUES = new List<ICAOCountry> { DE, RKS, GBD, GBN, GBO, GBP, GBS, XXA, XXB, XXC, XXX, EUE, UNO, UNA, UNK, XBA, XIM, XCC, XCO, XEC, XPO, XOM };
		}

		private ICAOCountry(string alpha2Code, string alpha3Code, string name)
			: this(alpha2Code, alpha3Code, name, name)
		{
		}

		private ICAOCountry(string alpha2Code, string alpha3Code, string name, string nationality)
		{
			Alpha2Code = alpha2Code;
			Alpha3Code = alpha3Code;
			Name = name;
			Nationality = nationality;
		}

		public string Name { get; }
		public string Nationality { get; }
		public string Alpha2Code { get; }
		public string Alpha3Code { get; }

		public static ICAOCountry GetInstance(string alpha3Code)
		{
			foreach (var c in VALUES)
			{
				if (c.Alpha3Code == alpha3Code) return c;
			}
			throw new System.ArgumentException($"Illegal ICAO country alpha 3 code {alpha3Code}");
		}

		public int ValueOf() => -1;
	}
}
