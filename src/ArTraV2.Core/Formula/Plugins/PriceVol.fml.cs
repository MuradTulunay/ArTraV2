#pragma warning disable
using ArTraV2.Core.Formula;



namespace FML
{
	#region Formula Group Price Volumn Indicators
	[System.Serializable]
	public class ASI:FormulaBase
	{
		public ASI():base()
		{
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData LC=REF(CLOSE,1); LC.Name="LC";
			FormulaData AA=ABS(HIGH-LC); AA.Name="AA";
			FormulaData BB=ABS(LOW-LC); BB.Name="BB";
			FormulaData CC=ABS(HIGH-REF(LOW,1)); CC.Name="CC";
			FormulaData DD=ABS(LC-REF(OPEN,1)); DD.Name="DD";
			FormulaData R=IF(AA>BB & AA>CC,AA+BB/2+DD/4,IF(BB>CC & BB>AA,BB+AA/2+DD/4,CC+DD/4)); R.Name="R";
			FormulaData X=(CLOSE-LC+(CLOSE-OPEN)/2+LC-REF(OPEN,1)); X.Name="X";
			FormulaData SI=16*X/R*MAX(AA,BB); SI.Name="SI";
			FormulaData ASI=SUM(SI,0); ASI.Name="ASI";
			
			return new FormulaPackage(new FormulaData[]{ASI},"");
		}
	
		public override string LongName
		{
			get{return "Accumulation Swing Index";}
		}
	
		public override string Description
		{
			get{return "The Accumulation Swing Index (ASI) is a cumulative sum of the Welles Wilder's Swing Index indicator";}
		}
		public override string OutputFields
		{
			get{return "ASI";}
		}
	} //class ASI

	[System.Serializable]
	public class OBV:FormulaBase
	{
		public double N=0;
		public OBV():base()
		{
			AddParam("N","20","1","100","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData OBV=SUM(IF(CLOSE>REF(CLOSE,1),VOL,IF(CLOSE<REF(CLOSE,1),-VOL,0)),0); OBV.Name="OBV";OBV.SetAttrs("Width2");
			FormulaData M=MA(OBV,N); M.Name="M";
			
			return new FormulaPackage(new FormulaData[]{OBV,M},"");
		}
	
		public override string LongName
		{
			get{return "On Balance Volume";}
		}
	
		public override string Description
		{
			get{return "Volume is the number of shares or contracts that change ownership over a given period of time.  It is an indication of supply and demand that is independent from price and can relate a great deal about the relative enthusiasm of buyers and sellers in the market place.  On Balance Volume is one indicator that is designed to track changes in volume over time.  It is the running total of volume calculated in such a way as to add the day's volume to a cumulative total if the day's close was higher than the previous day's close and to subtract the day's volume from the cumulative total on down days.  The assumption is that changes in volume will precede changes in price trend.  On Balance Volume was created by Joseph Granville and has a number of interpretive qualities and should be used in conjunction with other indications of price trend reversals.\n\nAnother use of On Balance Volume (OBV) is to look at the trend in the indicator.  A rising trend in the OBV gives sign of a healthy move into the security.  A doubtful trend, or sideways trend in the OBV leaves price trend suspect and a candidate for a reversal of the trend.  A falling OBV trend signals an exodus from the security despite price activity and leads to the caution that price may follow if it is not already.  As indicated on the graphs above, look for divergences between the price and the OBV indicator.  Divergences between the peaks warns of a potential fall in prices.  Divergences between the troughs warns of a potential rise in prices.";}
		}
		public override string OutputFields
		{
			get{return "OBV,M";}
		}
	} //class OBV

	[System.Serializable]
	public class PVT:FormulaBase
	{
		public PVT():base()
		{
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData NONAME0=SUM((CLOSE-REF(CLOSE,1))/REF(CLOSE,1)*VOL,0);
			return new FormulaPackage(new FormulaData[]{NONAME0},"");
		}
	
		public override string LongName
		{
			get{return "PRICE AND VOLUME TREND";}
		}
	
		public override string Description
		{
			get{return "Price and Volume Trend (PVT) is a variation of On Balance Volume, used to determine the strength of trends and warn of reversals.\r\n\r\nPVT = yesterday's PVT + today's Volume * (today's Close - yesterday's Close) / yesterday's Close\r\n";}
		}
		public override string OutputFields
		{
			get{return "SUM((CLOSE-REF(CLOSE,1))/REF(CLOSE,1)*VOL,0)";}
		}
	} //class PVT

	[System.Serializable]
	public class SOBV:FormulaBase
	{
		public SOBV():base()
		{
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData NONAME0=SUM(IF(ISUP,VOL,IF(ISDOWN,-VOL,0)),0);
			return new FormulaPackage(new FormulaData[]{NONAME0},"");
		}
	
		public override string LongName
		{
			get{return "SOBV";}
		}
	
		public override string Description
		{
			get{return "";}
		}
		public override string OutputFields
		{
			get{return "SUM(IF(ISUP,VOL,IF(ISDOWN,-VOL,0)),0)";}
		}
	} //class SOBV

	[System.Serializable]
	public class WVAD:FormulaBase
	{
		public double N1=0;
		public double N2=0;
		public WVAD():base()
		{
			AddParam("N1","10","1","100","1","",FormulaParamType.Double);
			AddParam("N2","20","1","100","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData WVAD= (CLOSE-OPEN)/(HIGH-LOW)*VOL; WVAD.Name="WVAD";
			FormulaData MA1=MA(WVAD,N1); MA1.Name="MA1";
			FormulaData MA2=MA(WVAD,N2); MA2.Name="MA2";
			
			return new FormulaPackage(new FormulaData[]{WVAD,MA1,MA2},"");
		}
	
		public override string LongName
		{
			get{return "Williams'Variable Accumulation Distribution";}
		}
	
		public override string Description
		{
			get{return "";}
		}
		public override string OutputFields
		{
			get{return "WVAD,MA1,MA2";}
		}
	} //class WVAD

	#endregion

} // namespace FML
