#pragma warning disable
using ArTraV2.Core.Formula;



namespace FML
{
	#region Formula Group Bands Indicators
	[System.Serializable]
	public class BBIBOLL:FormulaBase
	{
		public double N=0;
		public double P=0;
		public BBIBOLL():base()
		{
			AddParam("N","10","1","100","1","",FormulaParamType.Double);
			AddParam("P","3","0.1","20","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData BBI=(MA(CLOSE,3)+MA(CLOSE,6)+MA(CLOSE,12)+MA(CLOSE,24))/4; BBI.Name="BBI";
			FormulaData UPR=BBI+P*STD(BBI,N); UPR.Name="UPR";
			FormulaData DWN=BBI-P*STD(BBI,N); DWN.Name="DWN";
			return new FormulaPackage(new FormulaData[]{BBI,UPR,DWN},"");
		}
	
		public override string LongName
		{
			get{return "BBIBOLL";}
		}
	
		public override string Description
		{
			get{return "";}
		}
		public override string OutputFields
		{
			get{return "BBI,UPR,DWN";}
		}
	} //class BBIBOLL

	[System.Serializable]
	public class BOL:FormulaBase
	{
		public double N=0;
		public double P=0;
		public BOL():base()
		{
			AddParam("N","20","5","300","1","",FormulaParamType.Double);
			AddParam("P","2","0.1","10","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData BOLM=  MA(CLOSE,N); BOLM.Name="BOLM";
			FormulaData BOLU= BOLM + P*STD(CLOSE,N); BOLU.Name="BOLU";
			FormulaData BOLD= BOLM - P*STD(CLOSE,N); BOLD.Name="BOLD";
			FormulaData NONAME0=BOLU;
			FormulaData NONAME1=BOLM;
			FormulaData NONAME2=BOLD;
			return new FormulaPackage(new FormulaData[]{NONAME0,NONAME1,NONAME2},"");
		}
	
		public override string LongName
		{
			get{return "Bollinger Bands";}
		}
	
		public override string Description
		{
			get{return "The Bollinger Bands were introduced by John Bollinger. Its primary use is for presenting the volatility of the security in an easy to view form. The indicator consists of three bands: a simple moving average (middle), SMA plus 2 standard deviations (upper), and SMA minus 2 standard deviations (lower).";}
		}
		public override string OutputFields
		{
			get{return "BOLU,BOLM,BOLD";}
		}
	} //class BOL

	[System.Serializable]
	public class CDP:FormulaBase
	{
		public CDP():base()
		{
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData PT= REF(HIGH,1)-REF(LOW,1); PT.Name="PT";
			FormulaData CDP= (HIGH + LOW + CLOSE)/3; CDP.Name="CDP";
			FormulaData AH= CDP + PT; AH.Name="AH";
			FormulaData AL= CDP - PT; AL.Name="AL";
			FormulaData NH= 2*CDP-LOW; NH.Name="NH";
			FormulaData NL= 2*CDP-HIGH; NL.Name="NL";
			return new FormulaPackage(new FormulaData[]{CDP,AH,AL,NH,NL},"");
		}
	
		public override string LongName
		{
			get{return "CDP";}
		}
	
		public override string Description
		{
			get{return "";}
		}
		public override string OutputFields
		{
			get{return "CDP,AH,AL,NH,NL";}
		}
	} //class CDP

	[System.Serializable]
	public class ENV:FormulaBase
	{
		public double N=0;
		public ENV():base()
		{
			AddParam("N","14","2","300","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData UPPER= MA(CLOSE,N)*1.06; UPPER.Name="UPPER";
			FormulaData LOWER= MA(CLOSE,N)*0.94; LOWER.Name="LOWER";
			return new FormulaPackage(new FormulaData[]{UPPER,LOWER},"");
		}
	
		public override string LongName
		{
			get{return "ENVELOPES (TRADING BANDS)";}
		}
	
		public override string Description
		{
			get{return "An envelope is comprised of two moving averages. One moving average is shifted upward and the second moving average is shifted downward.\n\n";}
		}
		public override string OutputFields
		{
			get{return "UPPER,LOWER";}
		}
	} //class ENV

	[System.Serializable]
	public class MIKE:FormulaBase
	{
		public double N=0;
		public MIKE():base()
		{
			AddParam("N","12","1","200","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData TYP=(HIGH+LOW+CLOSE)/3; TYP.Name="TYP";
			FormulaData LL=LLV(LOW,N); LL.Name="LL";
			FormulaData HH=HHV(HIGH,N); HH.Name="HH";
			FormulaData WR=TYP+(TYP-LL); WR.Name="WR";
			FormulaData MR=TYP+(HH-LL); MR.Name="MR";
			FormulaData SR=2*HH-LL; SR.Name="SR";
			FormulaData WS=TYP-(HH-TYP); WS.Name="WS";
			FormulaData MS=TYP-(HH-LL); MS.Name="MS";
			FormulaData SS=2*LL-HH; SS.Name="SS";
			return new FormulaPackage(new FormulaData[]{WR,MR,SR,WS,MS,SS},"");
		}
	
		public override string LongName
		{
			get{return "MIKE";}
		}
	
		public override string Description
		{
			get{return "";}
		}
		public override string OutputFields
		{
			get{return "WR,MR,SR,WS,MS,SS";}
		}
	} //class MIKE

	[System.Serializable]
	public class SR:FormulaBase
	{
		public SR():base()
		{
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData M=(H+L+C)/3; M.Name="M";
			FormulaData A=H-L; A.Name="A";
			FormulaData RR=M+A; RR.Name="RR";
			FormulaData SS=M-A; SS.Name="SS";
			FormulaData R=BACKSET(ISLASTBAR,10)*RR.LASTDATA; R.Name="R";R.SetAttrs("Width2,HighSpeed,ColorRed");
			FormulaData S=BACKSET(ISLASTBAR,10)*SS.LASTDATA; S.Name="S";S.SetAttrs("Width2,HighSpeed,ColorDarkGreen");
			FormulaData NONAME0=DRAWNUMBER(BARSSINCE(R)==1,R,R,"f2");NONAME0.SetAttrs("Label0,VCenter,Right,ColorRed");
			FormulaData NONAME1=DRAWNUMBER(BARSSINCE(S)==1,S,S,"f2");NONAME1.SetAttrs("Label0,VCenter,Right,ColorDarkGreen");
			
			return new FormulaPackage(new FormulaData[]{R,S,NONAME0,NONAME1},"");
		}
	
		public override string LongName
		{
			get{return "Support & Resistance";}
		}
	
		public override string Description
		{
			get{return "";}
		}
		public override string OutputFields
		{
			get{return "R,S,DrawNumber(BarsSince(R)==1,R,R,\"f2\"),DrawNumber(BarsSince(S)==1,S,S,\"f2\")";}
		}
	} //class SR

	[System.Serializable]
	public class BBWidth:FormulaBase
	{
		public double N=0;
		public double P=0;
		public BBWidth():base()
		{
			AddParam("N","20","1","100","1","",FormulaParamType.Double);
			AddParam("P","2","0.1","100","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData NONAME0=P*STD(C,N)*2;NONAME0.SetAttrs("Width1.6,HighQuality");
			return new FormulaPackage(new FormulaData[]{NONAME0},"");
		}
	
		public override string LongName
		{
			get{return "Bollinger Band Width";}
		}
	
		public override string Description
		{
			get{return "The Bollinger Band Width indicator charts the width of the Bollinger Bands. When the Bollinger Band Width increases in value, it indicates that the volatility of the underlying stock has also increased.\n";}
		}
		public override string OutputFields
		{
			get{return "P*STD(C,N)*2";}
		}
	} //class BBWidth

	[System.Serializable]
	public class HHLLV:FormulaBase
	{
		public HHLLV():base()
		{
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData UPPER= HHV(H); UPPER.Name="UPPER";
			FormulaData LOWER= LLV(L); LOWER.Name="LOWER";
			return new FormulaPackage(new FormulaData[]{UPPER,LOWER},"");
		}
	
		public override string LongName
		{
			get{return "";}
		}
	
		public override string Description
		{
			get{return "En yüksek ve En düşük değerler\n\n";}
		}
		public override string OutputFields
		{
			get{return "UPPER,LOWER";}
		}
	} //class HHLLV

	[System.Serializable]
	public class HHV:FormulaBase
	{
		public HHV():base()
		{
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData UPPER= HHV(H); UPPER.Name="UPPER";
			
			return new FormulaPackage(new FormulaData[]{UPPER},"");
		}
	
		public override string LongName
		{
			get{return "";}
		}
	
		public override string Description
		{
			get{return "En yüksek ve En düşük değerler\n\n";}
		}
		public override string OutputFields
		{
			get{return "UPPER";}
		}
	} //class HHV

	#endregion

} // namespace FML
