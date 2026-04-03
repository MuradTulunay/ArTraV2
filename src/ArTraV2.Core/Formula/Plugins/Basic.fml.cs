#pragma warning disable
using ArTraV2.Core.Formula;



namespace FML
{
	#region Formula Group Others
	[System.Serializable]
	public class MASS:FormulaBase
	{
		public double N1=0;
		public double N2=0;
		public MASS():base()
		{
			AddParam("N1","9","2","100","1","",FormulaParamType.Double);
			AddParam("N2","25","5","100","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData NONAME0=SUM(EMA((HIGH-LOW),N1)/EMA(EMA((HIGH-LOW),N1),N1),N2);
			return new FormulaPackage(new FormulaData[]{NONAME0},"");
		}
	
		public override string LongName
		{
			get{return "Mass Index";}
		}
	
		public override string Description
		{
			get{return "The Mass Index was designed to identify trend reversals by measuring the narrowing and widening of the range between the high and low prices. As this range widens, the Mass Index increases; as the range narrows the Mass Index decreases.\n\nThe Mass Index was developed by Donald Dorsey.\n\n";}
		}
		public override string OutputFields
		{
			get{return "SUM(EMA((HIGH-LOW),N1)/EMA(EMA((HIGH-LOW),N1),N1),N2)";}
		}
	} //class MASS

	[System.Serializable]
	public class STD:FormulaBase
	{
		public double N=0;
		public STD():base()
		{
			AddParam("N","26","2","100","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData NONAME0=STD(CLOSE,N);
			return new FormulaPackage(new FormulaData[]{NONAME0},"");
		}
	
		public override string LongName
		{
			get{return "STD";}
		}
	
		public override string Description
		{
			get{return "STD";}
		}
		public override string OutputFields
		{
			get{return "STD(CLOSE,N)";}
		}
	} //class STD

	[System.Serializable]
	public class WAD:FormulaBase
	{
		public double N=0;
		public WAD():base()
		{
			AddParam("N","20","2","100","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData AD=SUM(IF( CLOSE>REF(CLOSE,1),CLOSE-MIN(REF(CLOSE,1),LOW),IF(CLOSE<REF(CLOSE,1),
CLOSE-MAX(REF(CLOSE,1),HIGH),0)),0); AD.Name="AD";
			FormulaData M=MA(AD,N); M.Name="M";
			
			return new FormulaPackage(new FormulaData[]{AD,M},"");
		}
	
		public override string LongName
		{
			get{return "William's Accumulation/Distribution";}
		}
	
		public override string Description
		{
			get{return "A price indicator attempting to assess the accumulation or distribution of securities.";}
		}
		public override string OutputFields
		{
			get{return "AD,M";}
		}
	} //class WAD

	[System.Serializable]
	public class ZigLabel:FormulaBase
	{
		public double N=0;
		public ZigLabel():base()
		{
			AddParam("N","6","1","100","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData NONAME0=DRAWNUMBER(FINDPEAK(N),H,H,"f2");NONAME0.SetAttrs("Label3");
			FormulaData NONAME1=DRAWNUMBER(FINDTROUGH(N),L,L,"f2");NONAME1.SetAttrs("Label3,Valign2");
			return new FormulaPackage(new FormulaData[]{NONAME0,NONAME1},"");
		}
	
		public override string LongName
		{
			get{return "Zig Zag Label";}
		}
	
		public override string Description
		{
			get{return "";}
		}
		public override string OutputFields
		{
			get{return "DrawNumber(FindPeak(N),H,H,\"f2\"),DrawNumber(FindTrough(N),L,L,\"f2\")";}
		}
	} //class ZigLabel

	[System.Serializable]
	public class PR:FormulaBase
	{
		public string STOCKCODE="";
		public double N=0;
		public PR():base()
		{
			AddParam("StockCode","^DJI","0","0","1","",FormulaParamType.String);
			AddParam("N","20","1","100","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData PR=C/FML("C")*100; PR.Name="PR";PR.SetAttrs("HighQuality");
			FormulaData NONAME0=EMA(PR,N);
			return new FormulaPackage(new FormulaData[]{PR,NONAME0},"");
		}
	
		public override string LongName
		{
			get{return "Price Relative";}
		}
	
		public override string Description
		{
			get{return "The Price Relative compares the performance of one security against that of another. It is often used to compare the performance of a particular stock to a market index, usually the S&P 500. Because the goal of many portfolio managers is to outperform the S&P 500, they are usually interested in the strongest stocks. The price relative offers a straightforward and accurate portrayal of a stock's performance relative to the market.";}
		}
		public override string OutputFields
		{
			get{return "PR,EMA(PR,N)";}
		}
	} //class PR

	[System.Serializable]
	public class Fibonnaci:FormulaBase
	{
		public double N=0;
		public Fibonnaci():base()
		{
			AddParam("N","100","1","100","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData A= HHV(H,N); A.Name="A";
			FormulaData B= LLV(L,N); B.Name="B";
			FormulaData HH=BACKSET(ISLASTBAR,N)*A.LASTDATA; HH.Name="HH";HH.SetAttrs("Width2");
			FormulaData LL=BACKSET(ISLASTBAR,N)*B.LASTDATA; LL.Name="LL";LL.SetAttrs("Width2");
			FormulaData HEIGHT= HH-LL; HEIGHT.Name="Height";
			FormulaData A1= LL+HEIGHT*0.382; A1.Name="A1";
			FormulaData A2= LL+HEIGHT*0.5; A2.Name="A2";
			FormulaData A3= LL+HEIGHT*0.618; A3.Name="A3";
			SETTEXTVISIBLE(HH,FALSE);
			SETTEXTVISIBLE(LL,FALSE);
			SETTEXTVISIBLE(A1,FALSE);
			SETTEXTVISIBLE(A2,FALSE);
			SETTEXTVISIBLE(A3,FALSE);
			return new FormulaPackage(new FormulaData[]{HH,LL,A1,A2,A3},"");
		}
	
		public override string LongName
		{
			get{return "Fibonnaci";}
		}
	
		public override string Description
		{
			get{return "";}
		}
		public override string OutputFields
		{
			get{return "HH,LL,A1,A2,A3";}
		}
	} //class Fibonnaci

	[System.Serializable]
	public class LinRegr:FormulaBase
	{
		public double N=0;
		public double P=0;
		public LinRegr():base()
		{
			AddParam("N","14","1","1000","1","",FormulaParamType.Double);
			AddParam("P","100","0","100","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData A= LR(C,N); A.Name="A";
			FormulaData DIST=C-A; DIST.Name="Dist";
			FormulaData M= MAX(MAXVALUE(DIST),ABS(MINVALUE(DIST)))*P/100; M.Name="M";
			FormulaData UPPER= A +M; UPPER.Name="Upper";
			FormulaData LOWER= A - M; LOWER.Name="Lower";
			FormulaData NONAME0=A;
			SETNAME(A,"");
			SETTEXTVISIBLE(UPPER,FALSE);
			SETTEXTVISIBLE(LOWER,FALSE);
			return new FormulaPackage(new FormulaData[]{UPPER,LOWER,NONAME0},"");
		}
	
		public override string LongName
		{
			get{return "Linear Regression Channels";}
		}
	
		public override string Description
		{
			get{return "";}
		}
		public override string OutputFields
		{
			get{return "Upper,Lower,A";}
		}
	} //class LinRegr

	[System.Serializable]
	public class RawData:FormulaBase
	{
		public string DATANAME="";
		public RawData():base()
		{
			AddParam("DataName","0","0","0","1","",FormulaParamType.String);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData O=ORGDATA(DATANAME); O.Name="O";
			SETNAME(O,DATANAME);
			SETTEXTVISIBLE(FALSE);
			return new FormulaPackage(new FormulaData[]{O},"");
		}
	
		public override string LongName
		{
			get{return "Provide raw data from data provider";}
		}
	
		public override string Description
		{
			get{return "";}
		}
		public override string OutputFields
		{
			get{return "O";}
		}
	} //class RawData

	[System.Serializable]
	public class ZigW:FormulaBase
	{
		public double N=0;
		public ZigW():base()
		{
			AddParam("N","10","1","100","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData NONAME0=ZIG(N);NONAME0.SetAttrs("Width2");
			FormulaData A=FINDPEAK(N); A.Name="A";
			FormulaData B=FINDTROUGH(N); B.Name="B";
			FormulaData A1=TOVALUE(A,H,0.5); A1.Name="A1";
			FormulaData A2=TOVALUE(B,L,0.5); A2.Name="A2";
			FormulaData NONAME1=POLYLINE(A,H);NONAME1.SetAttrs("StyleDash");
			FormulaData NONAME2=POLYLINE(B,L);NONAME2.SetAttrs("StyleDash,SameColor");
			FormulaData A3=ZIGP(N); A3.Name="A3";
			FormulaData NONAME3=DRAWNUMBER(A1,A1,A3,"f3");NONAME3.SetAttrs("Label3,VAlign0");
			FormulaData NONAME4=DRAWNUMBER(A2,A2,A3,"f3");NONAME4.SetAttrs("Label3,VAlign0");
			return new FormulaPackage(new FormulaData[]{NONAME0,NONAME1,NONAME2,NONAME3,NONAME4},"");
		}
	
		public override string LongName
		{
			get{return "Zig /w retracements";}
		}
	
		public override string Description
		{
			get{return "";}
		}
		public override string OutputFields
		{
			get{return "Zig(N),PolyLine(A,H),PolyLine(B,L),DrawNumber(A1,A1,A3,\"f3\"),DrawNumber(A2,A2,A3,\"f3\")";}
		}
	} //class ZigW

	[System.Serializable]
	public class ZigSR:FormulaBase
	{
		public double N=0;
		public ZigSR():base()
		{
			AddParam("N","1","0.0001","100","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData A1=PEAK(N); A1.Name="A1";
			FormulaData A2=PEAK(N,2); A2.Name="A2";
			FormulaData B1=PEAKBARS(N); B1.Name="B1";
			FormulaData B2=PEAKBARS(N,2); B2.Name="B2";
			FormulaData NONAME0=DRAWLINE(B2.LASTDATA,A2.LASTDATA,B1.LASTDATA,A1.LASTDATA,1);
			FormulaData NONAME1=DRAWTEXT(B2.LASTDATA,A2.LASTDATA," +hi[UP]");NONAME1.SetAttrs("SameColor");
			FormulaData NONAME2=DRAWTEXT(B1.LASTDATA,A1.LASTDATA," +hi");NONAME2.SetAttrs("SameColor");
			FormulaData D1=TROUGH(N); D1.Name="D1";
			FormulaData D2=TROUGH(N,2); D2.Name="D2";
			FormulaData E1=TROUGHBARS(N); E1.Name="E1";
			FormulaData E2=TROUGHBARS(N,2); E2.Name="E2";
			FormulaData NONAME3=DRAWLINE(E2.LASTDATA,D2.LASTDATA,E1.LASTDATA,D1.LASTDATA,1);NONAME3.SetAttrs("SameColor");
			FormulaData NONAME4=DRAWTEXT(E2.LASTDATA,D2.LASTDATA," +lo[up channel]");NONAME4.SetAttrs("VAlign2,SameColor");
			FormulaData NONAME5=DRAWTEXT(E1.LASTDATA,D1.LASTDATA," +lo[b/o retest]");NONAME5.SetAttrs("VAlign2,SameColor");
			
			return new FormulaPackage(new FormulaData[]{NONAME0,NONAME1,NONAME2,NONAME3,NONAME4,NONAME5},"");
		}
	
		public override string LongName
		{
			get{return "Zig support and resistance";}
		}
	
		public override string Description
		{
			get{return "";}
		}
		public override string OutputFields
		{
			get{return "DrawLine(B2.LastData,A2.LastData,B1.LastData,A1.LastData,1),DrawText(B2.LastData,A2.LastData,\" +hi[UP]\"),DrawText(B1.LastData,A1.LastData,\" +hi\"),DrawLine(E2.LastData,D2.LastData,E1.LastData,D1.LastData,1),DrawText(E2.LastData,D2.LastData,\" +lo[up channel]\"),DrawText(E1.LastData,D1.LastData,\" +lo[b/o retest]\")";}
		}
	} //class ZigSR

	[System.Serializable]
	public class ZigIcon:FormulaBase
	{
		public double N=0;
		public ZigIcon():base()
		{
			AddParam("N","6","0.01","100","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData NONAME0=DRAWICON(FINDPEAK(N),H,"buy.gif");NONAME0.SetAttrs("Top");
			FormulaData NONAME1=DRAWICON(FINDTROUGH(N),L,"sell.gif");NONAME1.SetAttrs("Bottom");
			return new FormulaPackage(new FormulaData[]{NONAME0,NONAME1},"");
		}
	
		public override string LongName
		{
			get{return "Draw buy sell Icon according Zig";}
		}
	
		public override string Description
		{
			get{return "";}
		}
		public override string OutputFields
		{
			get{return "DrawIcon(FindPeak(N),H,\"buy.gif\"),DrawIcon(FindTrough(N),L,\"sell.gif\")";}
		}
	} //class ZigIcon

	[System.Serializable]
	public class VHF:FormulaBase
	{
		public string F="";
		public double N=0;
		public VHF():base()
		{
			AddParam("F","C","C","V","1","",FormulaParamType.String);
			AddParam("N","28","3","1000","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData FD=IF(F=="O",O,IF(F=="H",H,IF(F=="L",L,IF(F=="V",V,IF(F=="C",C,C)
)
)
)
); FD.Name="fd";
			FormulaData AI=SETZERO(ABS(FD-REF(FD,1)),1); AI.Name="AI";
			FormulaData VHF=(HHV(FD,N)-LLV(FD,N))/SUM(AI,N); VHF.Name="VHF";
			FormulaData NONAME0=VHF;
			return new FormulaPackage(new FormulaData[]{NONAME0},"");
		}
	
		public override string LongName
		{
			get{return "Vertical Horizontal Filter (Easy Char Formülü)";}
		}
	
		public override string Description
		{
			get{return "Vertical Horizontal Filter (VHF) was created by Adam White to identify trending and ranging markets. VHF measures the level of trend activity, similar to ADX in the Directional Movement System. Trend indicators can then be employed in trending markets and momentum indicators in ranging markets.\n\nVary the number of periods in the Vertical Horizontal Filter to suit different time frames. White originally recommended 28 days but now prefers an 18-day window smoothed with a 6-day moving average.\n";}
		}
		public override string OutputFields
		{
			get{return "VHF";}
		}
	} //class VHF

	[System.Serializable]
	public class ZIGTP:FormulaBase
	{
		public ZIGTP():base()
		{
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData NONAME0=ZIG(CLOSE,1);
			return new FormulaPackage(new FormulaData[]{NONAME0},"");
		}
	
		public override string LongName
		{
			get{return "ZigZag Trend Prediction indicator";}
		}
	
		public override string Description
		{
			get{return "";}
		}
		public override string OutputFields
		{
			get{return "ZIGTP(1,1)";}
		}
	} //class ZIGTP

	[System.Serializable]
	public class WADC:FormulaBase
	{
		public WADC():base()
		{
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData AD=SUM(IF( CLOSE>REF(CLOSE,1),CLOSE-MIN(REF(CLOSE,1),LOW),IF(CLOSE<REF(CLOSE,1),
CLOSE-MAX(REF(CLOSE,1),HIGH),0)),0); AD.Name="AD";
			FormulaData WADC=C/AD; WADC.Name="WADC";
			return new FormulaPackage(new FormulaData[]{WADC},"");
		}
	
		public override string LongName
		{
			get{return "";}
		}
	
		public override string Description
		{
			get{return "A price indicator attempting to assess the accumulation or distribution of securities.";}
		}
		public override string OutputFields
		{
			get{return "WADC";}
		}
	} //class WADC

	#endregion

	#region Formula Group Native
	[System.Serializable]
	public class HL:FormulaBase
	{
		public double N=0;
		public HL():base()
		{
			AddParam("N","0","1","20000","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData NONAME0=N;
			return new FormulaPackage(new FormulaData[]{NONAME0},"");
		}
	
		public override string LongName
		{
			get{return "Horizon Line";}
		}
	
		public override string Description
		{
			get{return "";}
		}
		public override string OutputFields
		{
			get{return "N";}
		}
	} //class HL

	[System.Serializable]
	public class EMA:FormulaBase
	{
		public double N=0;
		public string F="";
		public EMA():base()
		{
			AddParam("N","12","1","1000","1","",FormulaParamType.Double);
			AddParam("F","C","C","V","1","Data Serisi veya indicatör ekleme",FormulaParamType.String);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData NONAME0=IF(F=="O",EMA(O,N),IF(F=="H",EMA(H,N),IF(F=="L",EMA(L,N),IF(F=="V",EMA(V,N),EMA(C,N))
)
)
);
			return new FormulaPackage(new FormulaData[]{NONAME0},"");
		}
	
		public override string LongName
		{
			get{return "EMA ";}
		}
	
		public override string Description
		{
			get{return "";}
		}
		public override string OutputFields
		{
			get{return "IF(F==\"O\",EMA(O,N),IF(F==\"H\",EMA(H,N),IF(F==\"L\",EMA(L,N),IF(F==\"V\",EMA(V,N),EMA(C,N))\n)\n)\n)";}
		}
	} //class EMA

	[System.Serializable]
	public class MA:FormulaBase
	{
		public double N=0;
		public MA():base()
		{
			AddParam("N","12","1","1000","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData NONAME0=MA(C,N);
			
			return new FormulaPackage(new FormulaData[]{NONAME0},"");
		}
	
		public override string LongName
		{
			get{return "MA";}
		}
	
		public override string Description
		{
			get{return "";}
		}
		public override string OutputFields
		{
			get{return "MA(C,N)";}
		}
	} //class MA

	[System.Serializable]
	public class MAIN:FormulaBase
	{
		public MAIN():base()
		{
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData M=STOCK; M.Name="M";
			SETNAME(M, STKLABEL);
			SETTEXTVISIBLE(FALSE);
			SETTEXTVISIBLE(M,FALSE);
			
			return new FormulaPackage(new FormulaData[]{M},"");
		}
	
		public override string LongName
		{
			get{return "Main View";}
		}
	
		public override string Description
		{
			get{return "";}
		}
		public override string OutputFields
		{
			get{return "M";}
		}
	} //class MAIN

	[System.Serializable]
	public class VOL:FormulaBase
	{
		public VOL():base()
		{
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData NONAME0=VOL;NONAME0.SetAttrs("VOLSTICK");
			return new FormulaPackage(new FormulaData[]{NONAME0},"");
		}
	
		public override string LongName
		{
			get{return "Volumn View";}
		}
	
		public override string Description
		{
			get{return "";}
		}
		public override string OutputFields
		{
			get{return "VOL";}
		}
	} //class VOL

	[System.Serializable]
	public class DotLine:FormulaBase
	{
		public double N=0;
		public DotLine():base()
		{
			AddParam("N","0","0","20000","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData NONAME0=N;NONAME0.SetAttrs("StyleDash,ColorRed");
			return new FormulaPackage(new FormulaData[]{NONAME0},"");
		}
	
		public override string LongName
		{
			get{return "Dot Horizon Line";}
		}
	
		public override string Description
		{
			get{return "";}
		}
		public override string OutputFields
		{
			get{return "N";}
		}
	} //class DotLine

	[System.Serializable]
	public class OverlayV:FormulaBase
	{
		public OverlayV():base()
		{
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData A=VOL; A.Name="A";A.SetAttrs("VOLSTICK,HIGH0.2,Alpha100");
			SETNAME(A,"V");
			SETTEXTVISIBLE(FALSE);
			return new FormulaPackage(new FormulaData[]{A},"");
		}
	
		public override string LongName
		{
			get{return "Volume for overlay";}
		}
	
		public override string Description
		{
			get{return "";}
		}
		public override string OutputFields
		{
			get{return "A";}
		}
	} //class OverlayV

	[System.Serializable]
	public class MAV:FormulaBase
	{
		public double N=0;
		public MAV():base()
		{
			AddParam("N","14","2","10000","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData MAV=MA(C,N); MAV.Name="MAV";
			return new FormulaPackage(new FormulaData[]{MAV},"");
		}
	
		public override string LongName
		{
			get{return "Moving Average (Matriks Excel)";}
		}
	
		public override string Description
		{
			get{return "";}
		}
		public override string OutputFields
		{
			get{return "MAV";}
		}
	} //class MAV

	#endregion

} // namespace FML
