#pragma warning disable
using ArTraV2.Core.Formula;



namespace FML
{
	#region Formula Group Index Indicators
	[System.Serializable]
	public class ABI:FormulaBase
	{
		public ABI():base()
		{
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData NONAME0=ABS(ADVANCE - DECLINE);
			return new FormulaPackage(new FormulaData[]{NONAME0},"");
		}
	
		public override string LongName
		{
			get{return "Absolute Breadth Index";}
		}
	
		public override string Description
		{
			get{return "The Absolute Breadth Index (\"ABI\") is a market momentum indicator that was developed by Norman G. Fosback.\n\nThe ABI shows how much activity, volatility, and change is taking place on the New York Stock Exchange while ignoring the direction prices are headed.\n\nYou can think of the ABI as an \"activity index.\" High readings indicate market activity and change, while low readings indicate lack of change.";}
		}
		public override string OutputFields
		{
			get{return "ABS(ADVANCE - DECLINE)";}
		}
	} //class ABI

	[System.Serializable]
	public class ADL:FormulaBase
	{
		public ADL():base()
		{
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData NONAME0=SUM(ADVANCE-DECLINE,0);
			return new FormulaPackage(new FormulaData[]{NONAME0},"");
		}
	
		public override string LongName
		{
			get{return "Advance/Decline line";}
		}
	
		public override string Description
		{
			get{return "The advance and decline line is a cumulative, ongoing sum of the difference between the number of stocks closing higher minus the number of stocks closing lower each day.  It can be used as a measure of market strength as it moves higher when the there are more advancing issues than declining issues.  It moves lower when there are more declining issues than advancing issues.  Plotting the Advance/Decline line allows insight into market strength.  At times the major U.S. indices can continue higher while we are seeing a drop in the advance/decline line.  This is called a divergence and warns that we may be at the end of an upward movement and sets the stage for a possible reversal of price trend.  However such divergences can exist over a long period of time before evidence of price trend reversal occur.  It becomes a matter of sound analysis to build as wide a body of evidence as possible in forming an outlook for the future path of prices.";}
		}
		public override string OutputFields
		{
			get{return "SUM(ADVANCE-DECLINE,0)";}
		}
	} //class ADL

	[System.Serializable]
	public class ADR:FormulaBase
	{
		public double N=0;
		public ADR():base()
		{
			AddParam("N","10","1","200","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData NONAME0=SUM(ADVANCE,N)/SUM(DECLINE,N);
			return new FormulaPackage(new FormulaData[]{NONAME0},"");
		}
	
		public override string LongName
		{
			get{return "ADVANCE/DECLINE RATIO";}
		}
	
		public override string Description
		{
			get{return "The Advance/Decline Ratio (\"A/D Ratio\") shows the ratio of advancing issues to declining issues. It is calculated by dividing the number of advancing issues by the number of declining issues.\n\nInterpretation\n\nThe A/D Ratio is similar to the Advancing-Declining Issues in that it displays market breadth. But, where the Advancing-Declining Issues subtracts the advancing/declining values, the A/D Ratio divides the values. The advantage of the Ratio is that it remains constant regardless of the number of issues that are traded on the New York Stock Exchange (which has steadily increased).\n\nA moving average of the A/D Ratio is often used as an overbought/oversold indicator. The higher the value, the more \"excessive\" the rally and the more likely a correction. Likewise, low readings imply an oversold market and suggest a technical rally.\n\nKeep in mind, however, that markets that appear to be extremely overbought or oversold may stay that way for some time. When investing using overbought and oversold indicators, it is wise to wait for the prices to confirm your belief that a change is due before placing your trades.\n\nDay-to-day fluctuations of the Advance/Decline Ratio are often eliminated by smoothing the ratio with a moving average\n";}
		}
		public override string OutputFields
		{
			get{return "SUM(ADVANCE,N)/SUM(DECLINE,N)";}
		}
	} //class ADR

	[System.Serializable]
	public class BT:FormulaBase
	{
		public double N=0;
		public BT():base()
		{
			AddParam("N","10","1","300","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData NONAME0=MA(ADVANCE/(ADVANCE-DECLINE),N);
			return new FormulaPackage(new FormulaData[]{NONAME0},"");
		}
	
		public override string LongName
		{
			get{return "Breadth Trust";}
		}
	
		public override string Description
		{
			get{return "The Breadth Thrust indicator is a market momentum indicator. It was developed by Dr. Martin Zweig. The Breadth Thrust is calculated by dividing a 10-day exponential moving average of the number of advancing issues, by the number of advancing plus declining issues.\n\nInterpretation\n\nA \"Breadth Thrust\" occurs when, during a 10-day period, the Breadth Thrust indicator rises from below 40% to above 61.5%. A \"Thrust\" indicates that the stock market has rapidly changed from an oversold condition to one of strength, but has not yet become overbought.\n\nAccording to Dr. Zweig, there have only been fourteen Breadth Thrusts since 1945. The average gain following these fourteen Thrusts was 24.6% in an average time-frame of eleven months. Dr. Zweig also points out that most bull markets begin with a Breadth Thrust.";}
		}
		public override string OutputFields
		{
			get{return "MA(ADVANCE/(ADVANCE-DECLINE),N)";}
		}
	} //class BT

	[System.Serializable]
	public class CHAIKIN:FormulaBase
	{
		public double LONG=0;
		public double SHORT=0;
		public CHAIKIN():base()
		{
			AddParam("LONG","10","5","300","1","",FormulaParamType.Double);
			AddParam("SHORT","3","1","300","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData ADL= SUM(ADVANCE-DECLINE,0); ADL.Name="ADL";
			FormulaData CHA=  MA(ADL,SHORT) - MA(ADL,LONG); CHA.Name="CHA";
			return new FormulaPackage(new FormulaData[]{CHA},"");
		}
	
		public override string LongName
		{
			get{return "Chaikin's Accumulation/Distribution Indicator";}
		}
	
		public override string Description
		{
			get{return "A cumulative volume indicator attempting to assess the net volume movement on a day or week.";}
		}
		public override string OutputFields
		{
			get{return "CHA";}
		}
	} //class CHAIKIN

	[System.Serializable]
	public class MCO:FormulaBase
	{
		public double N1=0;
		public double N2=0;
		public MCO():base()
		{
			AddParam("N1","19","10","80","1","",FormulaParamType.Double);
			AddParam("N2","39","30","100","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData NONAME0=EMA(ADVANCE-DECLINE,N1)/10 - EMA(ADVANCE-DECLINE,N2)/20;
			return new FormulaPackage(new FormulaData[]{NONAME0},"");
		}
	
		public override string LongName
		{
			get{return "McClellan Oscillator";}
		}
	
		public override string Description
		{
			get{return "The McClellan Oscillator can be used as an overbought/oversold indicator.  It also has value at predicting short term trend changes when it crosses the zero line. A rising indicator that crosses the zero line from below is a bullish sign.  A falling indicator that crosses the zero line from above is a bearish sign.\n\nThe McClellan Oscillator is calculated by subtracting a 39 day moving average of advances minus declines, from a 19 day moving average of advances minus declines.  Generally it is not considered a forward looking indicator but can tell you a lot about trend.\n";}
		}
		public override string OutputFields
		{
			get{return "EMA(ADVANCE-DECLINE,N1)/10 - EMA(ADVANCE-DECLINE,N2)/20";}
		}
	} //class MCO

	[System.Serializable]
	public class OBOS:FormulaBase
	{
		public double N=0;
		public OBOS():base()
		{
			AddParam("N","10","2","100","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData NONAME0=MA(ADVANCE-DECLINE,N);
			return new FormulaPackage(new FormulaData[]{NONAME0},"");
		}
	
		public override string LongName
		{
			get{return "Over Buy/Over Sell";}
		}
	
		public override string Description
		{
			get{return "";}
		}
		public override string OutputFields
		{
			get{return "MA(ADVANCE-DECLINE,N)";}
		}
	} //class OBOS

	[System.Serializable]
	public class STIX:FormulaBase
	{
		public double N=0;
		public STIX():base()
		{
			AddParam("N","11","1","100","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData NONAME0=EMA(ADVANCE/(ADVANCE+DECLINE)*100,N);
			return new FormulaPackage(new FormulaData[]{NONAME0},"");
		}
	
		public override string LongName
		{
			get{return "STIX";}
		}
	
		public override string Description
		{
			get{return "";}
		}
		public override string OutputFields
		{
			get{return "EMA(ADVANCE/(ADVANCE+DECLINE)*100,N)";}
		}
	} //class STIX

	#endregion

} // namespace FML
