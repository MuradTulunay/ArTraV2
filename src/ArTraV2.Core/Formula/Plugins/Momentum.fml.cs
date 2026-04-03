#pragma warning disable
using ArTraV2.Core.Formula;



namespace FML
{
	#region Formula Group Momentum Indicators
	[System.Serializable]
	public class B3612:FormulaBase
	{
		public B3612():base()
		{
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData B36= MA(CLOSE,3)-MA(CLOSE,6); B36.Name="B36";
			FormulaData B612= MA(CLOSE,6)-MA(CLOSE,12); B612.Name="B612";
			return new FormulaPackage(new FormulaData[]{B36,B612},"");
		}
	
		public override string LongName
		{
			get{return "Bias3-Bias6 and Bias6-Bias12";}
		}
	
		public override string Description
		{
			get{return "";}
		}
		public override string OutputFields
		{
			get{return "B36,B612";}
		}
	} //class B3612

	[System.Serializable]
	public class BIAS:FormulaBase
	{
		public double L1=0;
		public double L2=0;
		public double L3=0;
		public BIAS():base()
		{
			AddParam("L1","6","1","300","1","",FormulaParamType.Double);
			AddParam("L2","12","1","300","1","",FormulaParamType.Double);
			AddParam("L3","24","1","300","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData BIAS1= (CLOSE-MA(CLOSE,L1))/MA(CLOSE,L1)*100; BIAS1.Name="BIAS1";
			FormulaData BIAS2= (CLOSE-MA(CLOSE,L2))/MA(CLOSE,L2)*100; BIAS2.Name="BIAS2";
			FormulaData BIAS3= (CLOSE-MA(CLOSE,L3))/MA(CLOSE,L3)*100; BIAS3.Name="BIAS3";
			return new FormulaPackage(new FormulaData[]{BIAS1,BIAS2,BIAS3},"");
		}
	
		public override string LongName
		{
			get{return "BIAS";}
		}
	
		public override string Description
		{
			get{return "Show the distance of close and moving average.";}
		}
		public override string OutputFields
		{
			get{return "BIAS1,BIAS2,BIAS3";}
		}
	} //class BIAS

	[System.Serializable]
	public class CCI:FormulaBase
	{
		public double N=0;
		public CCI():base()
		{
			AddParam("N","14","2","100","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData TYP= (HIGH + LOW + CLOSE)/3; TYP.Name="TYP";
			FormulaData NONAME0=(TYP-MA(TYP,N))/(0.015*AVEDEV(TYP,N));
			return new FormulaPackage(new FormulaData[]{NONAME0},"");
		}
	
		public override string LongName
		{
			get{return "Commodity Channel Index";}
		}
	
		public override string Description
		{
			get{return "The Commodity Channel Index measures the position of price in relation to its moving average. This can be used to highlight when the market is overbought/oversold or to signal when a trend is weakening. The indicator is similar in concept to Bollinger Bands but is presented as an indicator line rather than as overbought/oversold levels.\n\nThe Commodity Channel Index was developed by Donald Lambert and is outlined in his book Commodities Channel Index: Tools for Trading Cyclic Trends.\n\n";}
		}
		public override string OutputFields
		{
			get{return "(TYP-MA(TYP,N))/(0.015*AVEDEV(TYP,N))";}
		}
	} //class CCI

	[System.Serializable]
	public class DBCD:FormulaBase
	{
		public double N=0;
		public double M=0;
		public double T=0;
		public DBCD():base()
		{
			AddParam("N","5","1","100","1","",FormulaParamType.Double);
			AddParam("M","16","1","100","1","",FormulaParamType.Double);
			AddParam("T","76","1","100","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData BIAS=(C-MA(C,N))/MA(C,N); BIAS.Name="BIAS";
			FormulaData DIF=(BIAS-REF(BIAS,M)); DIF.Name="DIF";
			FormulaData DBCD=SMA(DIF,T,1); DBCD.Name="DBCD";
			FormulaData MM=MA(DBCD,5); MM.Name="MM";
			return new FormulaPackage(new FormulaData[]{DBCD,MM},"");
		}
	
		public override string LongName
		{
			get{return "Bias Convergence Divergence";}
		}
	
		public override string Description
		{
			get{return "";}
		}
		public override string OutputFields
		{
			get{return "DBCD,MM";}
		}
	} //class DBCD

	[System.Serializable]
	public class DPO:FormulaBase
	{
		public DPO():base()
		{
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData NONAME0=C - REF(MA(CLOSE,20),11);
			return new FormulaPackage(new FormulaData[]{NONAME0},"");
		}
	
		public override string LongName
		{
			get{return "Detrended Price Oscillator";}
		}
	
		public override string Description
		{
			get{return "Detrended Price Oscillator compares closing price to a prior moving average, eliminating cycles longer than the moving average.\n\nThe real power of the Detrended Price Oscillator is in identifying turning points in longer cycles:\n\nWhen Detrended Price Oscillator shows a higher trough - expect an upturn in the intermediate cycle;\nWhen Detrended Price Oscillator experiences a lower peak - expect a downturn.\n";}
		}
		public override string OutputFields
		{
			get{return "C - REF(MA(CLOSE,20),11)";}
		}
	} //class DPO

	[System.Serializable]
	public class FastSTO:FormulaBase
	{
		public double N=0;
		public double M=0;
		public FastSTO():base()
		{
			AddParam("N","14","1","100","1","",FormulaParamType.Double);
			AddParam("M","3","2","40","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData K=(CLOSE-LLV(LOW,N))/(HHV(HIGH,N)-LLV(LOW,N))*100; K.Name="K";
			FormulaData D=MA(K,M); D.Name="D";
			return new FormulaPackage(new FormulaData[]{K,D},"");
		}
	
		public override string LongName
		{
			get{return "Fast Stochastic";}
		}
	
		public override string Description
		{
			get{return "The Stochastic Oscillator is a measure of the relative momentum of current prices to previous closing prices within a given interval.  When it is plotted, it is two lines that move within a range of 0 and 100.  Values above 80 are considered to be in overbought territory giving an indication that a reversal in price is possible.  Values below 20 are considered oversold and again are an indication that a reversal of the price trend is a higher risk.  In a strong trending environment, the Stochastic Oscillator can stay in overbought or oversold territory for some time while price continues in a single direction.  In relation to a longer term price trend environment, the stochastic provides little interest.  In its construction it is meant to relate the current periods momentum to the most recent previous periods of momentum in price in an attempt to identify periods where momentum may be easing or increasing.  The easing (at a top) or increase (at a bottom) of momentum occurs at reversal points for the price trend being measured.  However changing momentum also occurs during times when there is no change in the overall trend in prices and should be understood as a period when a reversal in price trend is possible but not guaranteed.";}
		}
		public override string OutputFields
		{
			get{return "K,D";}
		}
	} //class FastSTO

	[System.Serializable]
	public class LWR:FormulaBase
	{
		public double N=0;
		public double M1=0;
		public double M2=0;
		public LWR():base()
		{
			AddParam("N","9","1","100","1","",FormulaParamType.Double);
			AddParam("M1","3","2","40","1","",FormulaParamType.Double);
			AddParam("M2","3","2","40","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData RSV= (HHV(HIGH,N)-CLOSE)/(HHV(HIGH,N)-LLV(LOW,N))*100; RSV.Name="RSV";
			FormulaData LWR1=SMA(RSV,M1,1); LWR1.Name="LWR1";
			FormulaData LWR2=SMA(LWR1,M2,1); LWR2.Name="LWR2";
			return new FormulaPackage(new FormulaData[]{LWR1,LWR2},"");
		}
	
		public override string LongName
		{
			get{return "";}
		}
	
		public override string Description
		{
			get{return "100-Stochastic";}
		}
		public override string OutputFields
		{
			get{return "LWR1,LWR2";}
		}
	} //class LWR

	[System.Serializable]
	public class ROC:FormulaBase
	{
		public double N=0;
		public double MM=0;
		public ROC():base()
		{
			AddParam("N","12","1","100","1","",FormulaParamType.Double);
			AddParam("MM","6","1","50","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData ROC=(CLOSE-REF(CLOSE,N))/REF(CLOSE,N)*100; ROC.Name="ROC";
			FormulaData ROCMA=MA(ROC,MM); ROCMA.Name="ROCMA";
			return new FormulaPackage(new FormulaData[]{ROC,ROCMA},"");
		}
	
		public override string LongName
		{
			get{return "Price Rate-of-Change";}
		}
	
		public override string Description
		{
			get{return "ROC is a refinement of Momentum - readings fluctuate as percentages around the zero line. Further details are given at Construction.\n\nThe indicator is designed for use in ranging markets - to detect trend weakness and likely reversal points. However, when combined with a trend indicator, it can be used in trending markets.\n\nRanging Markets\nFirst, you will need to set overbought and oversold levels based on your observation of past ranging markets. The levels should cut across at least two-thirds of the peaks and troughs.\n\nGo long when ROC crosses to below the oversold level and then rises back above it.\nGo long on bullish divergences - where the first trough is below the oversold level.\nGo short when ROC crosses to above the overbought level and then falls back below it.\nGo short on a bearish divergence - with the first peak above the overbought level.\n";}
		}
		public override string OutputFields
		{
			get{return "ROC,ROCMA";}
		}
	} //class ROC

	[System.Serializable]
	public class RSI:FormulaBase
	{
		public double N1=0;
		public RSI():base()
		{
			AddParam("N1","14","2","100","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData LC= REF(CLOSE,1); LC.Name="LC";
			FormulaData RSI=SMA(MAX(CLOSE-LC,0),N1,1)/SMA(ABS(CLOSE-LC),N1,1)*100; RSI.Name="RSI";
			return new FormulaPackage(new FormulaData[]{RSI},"");
		}
	
		public override string LongName
		{
			get{return "Relative Strength Index";}
		}
	
		public override string Description
		{
			get{return "The Relative Strength Index was introduced by Welles Wilder. It is an indicator for overbought / oversold conditions. It is going up when the market is strong, and down, when the market is weak. The oscillation range is between 0 and 100.\n\nThe indicator is non-linear, it is moving faster in the middle of its range, and slower - in the overbought / oversold territory.\n\nThe RSI should not be confused with the relative strength indicator which is used to compare stocks to each other.\n\n";}
		}
		public override string OutputFields
		{
			get{return "RSI";}
		}
	} //class RSI

	[System.Serializable]
	public class SI:FormulaBase
	{
		public SI():base()
		{
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData LC=REF(C,1); LC.Name="LC";
			FormulaData AA=ABS(H-LC); AA.Name="AA";
			FormulaData BB=ABS(L-LC); BB.Name="BB";
			FormulaData CC=ABS(H-REF(L,1)); CC.Name="CC";
			FormulaData DD=ABS(LC-REF(O,1)); DD.Name="DD";
			FormulaData R=IF(AA>BB & AA>CC,AA+BB/2+DD/4,IF(BB>CC & BB>AA,BB+AA/2+DD/4,CC+DD/4)); R.Name="R";
			FormulaData X=(C-LC+(C-O)/2+LC-REF(O,1)); X.Name="X";
			FormulaData SI=16*X/R*MAX(AA,BB); SI.Name="SI";
			return new FormulaPackage(new FormulaData[]{SI},"");
		}
	
		public override string LongName
		{
			get{return "";}
		}
	
		public override string Description
		{
			get{return "Swing Index\n";}
		}
		public override string OutputFields
		{
			get{return "SI";}
		}
	} //class SI

	[System.Serializable]
	public class SlowSTO:FormulaBase
	{
		public double N=0;
		public double M1=0;
		public double M2=0;
		public SlowSTO():base()
		{
			AddParam("N","14","1","100","1","",FormulaParamType.Double);
			AddParam("M1","3","2","50","1","",FormulaParamType.Double);
			AddParam("M2","3","2","50","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData A=(CLOSE-LLV(LOW,N))/(HHV(HIGH,N)-LLV(LOW,N))*100; A.Name="A";
			FormulaData K=MA(A,M1); K.Name="K";
			FormulaData D=MA(K,M2); D.Name="D";
			return new FormulaPackage(new FormulaData[]{K,D},"");
		}
	
		public override string LongName
		{
			get{return "Slow Stochastic";}
		}
	
		public override string Description
		{
			get{return "The Slow Stochastic applies further smoothing to the Stochastic oscillator, to reduce volatility and improve signal accuracy.";}
		}
		public override string OutputFields
		{
			get{return "K,D";}
		}
	} //class SlowSTO

	[System.Serializable]
	public class SRDM:FormulaBase
	{
		public double N=0;
		public SRDM():base()
		{
			AddParam("N","30","1","100","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData DMZ=IF((H+L)<=(REF(H,1)+REF(L,1)),0,MAX(ABS(H-REF(H,1)),ABS(L-REF(L,1)))); DMZ.Name="DMZ";
			FormulaData DMF=IF((H+L)>=(REF(H,1)+REF(L,1)),0,MAX(ABS(H-REF(H,1)),ABS(L-REF(L,1)))); DMF.Name="DMF";
			FormulaData ADMZ=MA(DMZ,10); ADMZ.Name="ADMZ";
			FormulaData ADMF=MA(DMF,10); ADMF.Name="ADMF";
			FormulaData SRDM=IF(ADMZ>ADMF,(ADMZ-ADMF)/ADMZ,IF(ADMZ=ADMF,0,(ADMZ-ADMF)/ADMF)); SRDM.Name="SRDM";
			FormulaData ASRDM=SMA(SRDM,N,1); ASRDM.Name="ASRDM";
			return new FormulaPackage(new FormulaData[]{SRDM,ASRDM},"");
		}
	
		public override string LongName
		{
			get{return "SRDM";}
		}
	
		public override string Description
		{
			get{return "";}
		}
		public override string OutputFields
		{
			get{return "SRDM,ASRDM";}
		}
	} //class SRDM

	[System.Serializable]
	public class VROC:FormulaBase
	{
		public double N=0;
		public VROC():base()
		{
			AddParam("N","12","2","300","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData NONAME0=(VOL-REF(VOL,N))/REF(VOL,N)*100;
			return new FormulaPackage(new FormulaData[]{NONAME0},"");
		}
	
		public override string LongName
		{
			get{return "Rate of Change (Volume)";}
		}
	
		public override string Description
		{
			get{return "Rate of Change Volume (ROCV) is an oscillator applied to volume rather than price and is calculated in the same manner as the Rate of Change (Price) indicator.\n\nROCV highlights increases in volume, which normally occur at most significant market tops, bottoms and breakouts.\n\n";}
		}
		public override string OutputFields
		{
			get{return "(VOL-REF(VOL,N))/REF(VOL,N)*100";}
		}
	} //class VROC

	[System.Serializable]
	public class VRSI:FormulaBase
	{
		public double N=0;
		public VRSI():base()
		{
			AddParam("N","6","1","100","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData NONAME0=SMA(MAX(VOL-REF(VOL,1),0),N,1)/SMA(ABS(VOL-REF(VOL,1)),N,1)*100;
			return new FormulaPackage(new FormulaData[]{NONAME0},"");
		}
	
		public override string LongName
		{
			get{return "Volumn Relative Strength Index";}
		}
	
		public override string Description
		{
			get{return "Volumn RSI";}
		}
		public override string OutputFields
		{
			get{return "SMA(MAX(VOL-REF(VOL,1),0),N,1)/SMA(ABS(VOL-REF(VOL,1)),N,1)*100";}
		}
	} //class VRSI

	[System.Serializable]
	public class WR:FormulaBase
	{
		public double N=0;
		public WR():base()
		{
			AddParam("N","14","2","100","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData NONAME0=-100*(HHV(HIGH,N)-CLOSE)/(HHV(HIGH,N)-LLV(LOW,N));
			return new FormulaPackage(new FormulaData[]{NONAME0},"");
		}
	
		public override string LongName
		{
			get{return "Williams %R";}
		}
	
		public override string Description
		{
			get{return "The The Williams %R indicator was introduced by Larry Williams. It is working by identifying the overbought / oversold levels. The scale extends from 0 to -100.\n\nUsage\nThe overbought level is considered 0 to -20, and oversold -70 to -100.\n\nAs a confirmation signal, we can wait for the indicator to cross the -50 line.\n";}
		}
		public override string OutputFields
		{
			get{return "-100*(HHV(HIGH,N)-CLOSE)/(HHV(HIGH,N)-LLV(LOW,N))";}
		}
	} //class WR

	[System.Serializable]
	public class SO:FormulaBase
	{
		public double N1=0;
		public double N2=0;
		public SO():base()
		{
			AddParam("N1","3","1","10","1","",FormulaParamType.Double);
			AddParam("N2","3","1","10","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData K=(C-L)/(H-L); K.Name="K";
			FormulaData SK= SMA(K,N1,1); SK.Name="SK";
			FormulaData SD= SMA(SK,N2,1); SD.Name="SD";
			return new FormulaPackage(new FormulaData[]{SK,SD},"");
		}
	
		public override string LongName
		{
			get{return "Stochastic Oscillator";}
		}
	
		public override string Description
		{
			get{return "The Stochastic Oscillator was introduced by George C. Lane. The indicator provides information about the location of a current close in relation to the period's high and low. The closer the close is to the period's high, the higher is the buying pressure, and the closer the close is to the period's low, the more selling pressure is.\n\nUsage:\n\nThe indicator is considered bullish, when above 80, and bearish, when below 20. As this definition does not provide any insights on when to buy or sell, consider generating signals when the indicator moves from the overbought / oversold territory back.\n\nThe crossings between the %K and its moving average can be used for the same purpose.\n\nFinally, the divergence can be considered a very strong signal. When the divergence develops when the indicator is moving from the overbought / oversold levels, it is a sell / buy signal.\n\nAdditionally, the K39 (unsmoothed 39 period stochastic oscillator) was reported to generate good results when tested on paper. A buy signal is generated when K crosses above 50% and the close price is above the previous week's highest close. Sell signals are generated when K crosses below 50% and the close is below the previous week's lowest close.\n\nAn additional confirmation can be provided by some indicators from the different group, for example, the On Balance Volume (OBV) indicator.\n\nThe most value of a stochastics is when the strong trend is present. According to Lane, the safest way to trade is to buy when the trend is up, and to sell with the downtrend.\n";}
		}
		public override string OutputFields
		{
			get{return "SK,SD";}
		}
	} //class SO

	[System.Serializable]
	public class MFI:FormulaBase
	{
		public double N=0;
		public MFI():base()
		{
			AddParam("N","14","1","100","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData TP=(HIGH + LOW + CLOSE) / 3; TP.Name="TP";
			FormulaData MF=TP*V; MF.Name="MF";
			FormulaData LTP= REF(TP,1); LTP.Name="LTP";
			FormulaData PMF= IF(TP>=LTP,TP,0); PMF.Name="PMF";
			FormulaData NMF= IF(TP<LTP,TP,0); NMF.Name="NMF";
			FormulaData MR=SMA(PMF,N,1)/SMA(NMF,N,1); MR.Name="MR";
			FormulaData MFI=100-(100/(1+MR)); MFI.Name="MFI";
			return new FormulaPackage(new FormulaData[]{MFI},"");
		}
	
		public override string LongName
		{
			get{return "Money Flow";}
		}
	
		public override string Description
		{
			get{return "";}
		}
		public override string OutputFields
		{
			get{return "MFI";}
		}
	} //class MFI

	[System.Serializable]
	public class StochRSI:FormulaBase
	{
		public double N=0;
		public StochRSI():base()
		{
			AddParam("N","14","1","100","1","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData LC= REF(CLOSE,1); LC.Name="LC";
			FormulaData RSI=SMA(MAX(CLOSE-LC,0),N,1)/SMA(ABS(CLOSE-LC),N,1)*100; RSI.Name="RSI";
			FormulaData NONAME0=(RSI-LLV(RSI,N))/(HHV(RSI,N)-LLV(RSI,N));
			return new FormulaPackage(new FormulaData[]{NONAME0},"");
		}
	
		public override string LongName
		{
			get{return "StochRSI";}
		}
	
		public override string Description
		{
			get{return "Developed by Tushard Chande and Stanley Kroll, StochRSI is an oscillator that measures the level of RSI relative to its range, over a set period of time. The indicator uses RSI as the foundation and applies to it the formula behind Stochastics. The result is an oscillator that fluctuates between 0 and 1.";}
		}
		public override string OutputFields
		{
			get{return "(RSI-LLV(RSI,N))/(HHV(RSI,N)-LLV(RSI,N))";}
		}
	} //class StochRSI

	[System.Serializable]
	public class RSIA:FormulaBase
	{
		public double N1=0;
		public double L1=0;
		public double L2=0;
		public double L3=0;
		public double L4=0;
		public RSIA():base()
		{
			AddParam("N1","14","2","100","1","",FormulaParamType.Double);
			AddParam("L1","-66","-100","100","1","",FormulaParamType.Double);
			AddParam("L2","-33","-100","100","1","",FormulaParamType.Double);
			AddParam("L3","33","-100","100","1","",FormulaParamType.Double);
			AddParam("L4","66","-100","100","1","RSI Trade formülleri için base formula",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData NONAME0=FML(DP,"RSI(N1)");
			FormulaData S1=L1; S1.Name="S1";
			FormulaData S2=L2; S2.Name="S2";
			FormulaData S3=L3; S3.Name="S3";
			FormulaData S4=L4; S4.Name="S4";
			
			return new FormulaPackage(new FormulaData[]{NONAME0,S1,S2,S3,S4},"");
		}
	
		public override string LongName
		{
			get{return "";}
		}
	
		public override string Description
		{
			get{return "RSI ve destek direnç çizgisi";}
		}
		public override string OutputFields
		{
			get{return "FML(DP,\"RSI(N1)\"),S1,S2,S3,S4";}
		}
	} //class RSIA

	[System.Serializable]
	public class FISHER:FormulaBase
	{
		public double LOOKBACK=0;
		public double SMOOTH_PERIOD=0;
		public double SIGNAL_PERIOD=0;
		public double FAST_PERIOD=0;
		public double SLOW_PERIOD=0;
		public double OVERBOUGHT_LEVEL=0;
		public double OVERSOLD_LEVEL=0;
		public double SENSITIVITY=0;
		public double ONMAIN=0;
		public double STOPSPREAD=0;
		public FISHER():base()
		{
			AddParam("LOOKBACK","14","5","50","1","Ana Lookback Periyodu",FormulaParamType.Double);
			AddParam("SMOOTH_PERIOD","5","2","20","1","Düzgünleştirme Periyodu",FormulaParamType.Double);
			AddParam("SIGNAL_PERIOD","3","2","10","1","Sinyal Hattı Periyodu",FormulaParamType.Double);
			AddParam("FAST_PERIOD","7","3","21","1","Hızlı Fisher Periyodu",FormulaParamType.Double);
			AddParam("SLOW_PERIOD","28","14","100","1","Yavaş Fisher Periyodu",FormulaParamType.Double);
			AddParam("OVERBOUGHT_LEVEL","2","1","5","0.5","Aşırı Alım Seviyesi",FormulaParamType.Double);
			AddParam("OVERSOLD_LEVEL","-2","-5","-1","0.5","Aşırı Satım Seviyesi",FormulaParamType.Double);
			AddParam("SENSITIVITY","2","1","3","1","Sinyal Hassasiyeti",FormulaParamType.Double);
			AddParam("onMain","0","0","1","1","",FormulaParamType.Double);
			AddParam("StopSpread","0","0.0","5","0.01","",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData HL2=(HIGH+LOW)/2; HL2.Name="HL2";
			FormulaData MINHL=LLV(HL2,LOOKBACK); MINHL.Name="MinHL";
			FormulaData MAXHL=HHV(HL2,LOOKBACK); MAXHL.Name="MaxHL";
			FormulaData RAW=2*((HL2-MINHL)/(MAXHL-MINHL)-0.5); RAW.Name="RAW";
			FormulaData SMOOTH=EMA(RAW,SMOOTH_PERIOD); SMOOTH.Name="SMOOTH";
			FormulaData FISHER_BASIC=0.5*LN((1+SMOOTH)/(1-SMOOTH)); FISHER_BASIC.Name="FISHER_BASIC";
			FormulaData FISHER_LINE=EMA(FISHER_BASIC,SIGNAL_PERIOD); FISHER_LINE.Name="FISHER_LINE";
			FormulaData TRIGGER_LINE=REF(FISHER_LINE,1); TRIGGER_LINE.Name="TRIGGER_LINE";
			FormulaData HL2_FAST=(HIGH+LOW)/2; HL2_FAST.Name="HL2_FAST";
			FormulaData MINHL_FAST=LLV(HL2_FAST,FAST_PERIOD); MINHL_FAST.Name="MinHL_FAST";
			FormulaData MAXHL_FAST=HHV(HL2_FAST,FAST_PERIOD); MAXHL_FAST.Name="MaxHL_FAST";
			FormulaData RAW_FAST=2*((HL2_FAST-MINHL_FAST)/(MAXHL_FAST-MINHL_FAST)-0.5); RAW_FAST.Name="RAW_FAST";
			FormulaData SMOOTH_FAST=EMA(RAW_FAST,3); SMOOTH_FAST.Name="SMOOTH_FAST";
			FormulaData FISHER_FAST=0.5*LN((1+SMOOTH_FAST)/(1-SMOOTH_FAST)); FISHER_FAST.Name="FISHER_FAST";
			FormulaData HL2_SLOW=(HIGH+LOW)/2; HL2_SLOW.Name="HL2_SLOW";
			FormulaData MINHL_SLOW=LLV(HL2_SLOW,SLOW_PERIOD); MINHL_SLOW.Name="MinHL_SLOW";
			FormulaData MAXHL_SLOW=HHV(HL2_SLOW,SLOW_PERIOD); MAXHL_SLOW.Name="MaxHL_SLOW";
			FormulaData RAW_SLOW=2*((HL2_SLOW-MINHL_SLOW)/(MAXHL_SLOW-MINHL_SLOW)-0.5); RAW_SLOW.Name="RAW_SLOW";
			FormulaData SMOOTH_SLOW=EMA(RAW_SLOW,SMOOTH_PERIOD*2); SMOOTH_SLOW.Name="SMOOTH_SLOW";
			FormulaData FISHER_SLOW=0.5*LN((1+SMOOTH_SLOW)/(1-SMOOTH_SLOW)); FISHER_SLOW.Name="FISHER_SLOW";
			FormulaData FISHER_TREND=EMA(FISHER_SLOW,SIGNAL_PERIOD*2); FISHER_TREND.Name="FISHER_TREND";
			FormulaData BASIC_BUY=CROSS(FISHER_LINE,TRIGGER_LINE); BASIC_BUY.Name="BASIC_BUY";
			FormulaData BASIC_SELL=CROSS(TRIGGER_LINE,FISHER_LINE); BASIC_SELL.Name="BASIC_SELL";
			FormulaData OVERBOUGHT=FISHER_LINE>OVERBOUGHT_LEVEL; OVERBOUGHT.Name="OVERBOUGHT";
			FormulaData OVERSOLD=FISHER_LINE<OVERSOLD_LEVEL; OVERSOLD.Name="OVERSOLD";
			FormulaData OB_REVERSE=REF(OVERBOUGHT,1) & FISHER_LINE<OVERBOUGHT_LEVEL; OB_REVERSE.Name="OB_REVERSE";
			FormulaData OS_REVERSE=REF(OVERSOLD,1) & FISHER_LINE>OVERSOLD_LEVEL; OS_REVERSE.Name="OS_REVERSE";
			FormulaData MOMENTUM_UP=FISHER_FAST>REF(FISHER_FAST,1) & FISHER_FAST>0; MOMENTUM_UP.Name="MOMENTUM_UP";
			FormulaData MOMENTUM_DOWN=FISHER_FAST<REF(FISHER_FAST,1) & FISHER_FAST<0; MOMENTUM_DOWN.Name="MOMENTUM_DOWN";
			FormulaData TREND_BULLISH=FISHER_TREND>0 & FISHER_TREND>REF(FISHER_TREND,2); TREND_BULLISH.Name="TREND_BULLISH";
			FormulaData TREND_BEARISH=FISHER_TREND<0 & FISHER_TREND<REF(FISHER_TREND,2); TREND_BEARISH.Name="TREND_BEARISH";
			FormulaData FAST_CONFIRMATION=FISHER_FAST>0; FAST_CONFIRMATION.Name="FAST_CONFIRMATION";
			FormulaData SLOW_CONFIRMATION=FISHER_FAST<=0; SLOW_CONFIRMATION.Name="SLOW_CONFIRMATION";
			FormulaData LEVEL1_BUY=BASIC_BUY | OS_REVERSE; LEVEL1_BUY.Name="LEVEL1_BUY";
			FormulaData LEVEL1_SELL=BASIC_SELL | OB_REVERSE; LEVEL1_SELL.Name="LEVEL1_SELL";
			FormulaData LEVEL2_BUY=LEVEL1_BUY & MOMENTUM_UP; LEVEL2_BUY.Name="LEVEL2_BUY";
			FormulaData LEVEL2_SELL=LEVEL1_SELL & MOMENTUM_DOWN; LEVEL2_SELL.Name="LEVEL2_SELL";
			FormulaData LEVEL3_BUY=LEVEL2_BUY & TREND_BULLISH & FAST_CONFIRMATION; LEVEL3_BUY.Name="LEVEL3_BUY";
			FormulaData LEVEL3_SELL=LEVEL2_SELL & TREND_BEARISH & SLOW_CONFIRMATION; LEVEL3_SELL.Name="LEVEL3_SELL";
			FormulaData BUY=IF(SENSITIVITY==1,LEVEL1_BUY,IF(SENSITIVITY==2,LEVEL2_BUY,LEVEL3_BUY)); BUY.Name="BUY";
			FormulaData SELL=IF(SENSITIVITY==1,LEVEL1_SELL,IF(SENSITIVITY==2,LEVEL2_SELL,LEVEL3_SELL)); SELL.Name="SELL";
			FormulaData NONAME0=FISHER_LINE;NONAME0.SetAttrs(" Blue, Width2");
			FormulaData NONAME1=TRIGGER_LINE;NONAME1.SetAttrs(" Red, Width1");
			FormulaData NONAME2=FISHER_TREND;NONAME2.SetAttrs(" Green, Width1");
			FormulaData NONAME3=OVERBOUGHT_LEVEL;NONAME3.SetAttrs(" DarkRed, Width1");
			FormulaData NONAME4=0;NONAME4.SetAttrs(" Gray, Width1");
			FormulaData NONAME5=OVERSOLD_LEVEL;NONAME5.SetAttrs(" DarkGreen, Width1");
			FormulaData ALTBUY=EMPTY; ALTBUY.Name="ALTBUY";
			FormulaData ALTSELL=EMPTY; ALTSELL.Name="ALTSELL";
			FormulaData OPENSELL=EMPTY; OPENSELL.Name="OPENSELL";OPENSELL.SetAttrs("ColorDarkOrange");
			FormulaData CLOSEOPENSELL=EMPTY; CLOSEOPENSELL.Name="CLOSEOPENSELL";CLOSEOPENSELL.SetAttrs("ColorDarkViolet");
			FormulaData STOPLOSS=EMPTY; STOPLOSS.Name="STOPLOSS";
			FormulaData BALANCE= 10000.0; BALANCE.Name="BALANCE";
			FormulaData BT= IF(ONMAIN, C, BACKTEST(C,BUY,SELL,OPENSELL,CLOSEOPENSELL,BALANCE[0],0.000342,1,ALTBUY,ALTSELL,STOPSPREAD,STOPLOSS,0.0,FALSE)); BT.Name="BT";
			FormulaData NONAME6=DRAWICON(BUY, IF(ONMAIN, C, BT) ,"Buy.Gif");NONAME6.SetAttrs("Bottom");
			FormulaData NONAME7=DRAWICON(SELL, IF(ONMAIN, C, BT) ,"Sell.Gif");NONAME7.SetAttrs("Top");
			FormulaData NONAME8=DRAWICON(OPENSELL, IF(ONMAIN, C, BT) ,"OpenSell.Gif");NONAME8.SetAttrs("Top");
			FormulaData NONAME9=DRAWICON(CLOSEOPENSELL, IF(ONMAIN, C, BT) ,"CloseOpenSell.Gif");NONAME9.SetAttrs("Bottom");
			FormulaData NONAME10=DRAWICON(IF(ONMAIN, 0, ALTBUY), BT ,"AltBuy.Gif");NONAME10.SetAttrs("Bottom");
			FormulaData NONAME11=DRAWICON(IF(ONMAIN, 0, ALTSELL), BT ,"AltSell.Gif");NONAME11.SetAttrs("Top");
			FormulaData STOPBUYS= STOPLOSS>0; STOPBUYS.Name="STOPBUYS";
			FormulaData STOPSELLS=STOPLOSS<0; STOPSELLS.Name="STOPSELLS";
			FormulaData NONAME12=DRAWICON(IF(ONMAIN, 0, STOPBUYS), BT ,"StopBuy.Gif");NONAME12.SetAttrs("Bottom");
			FormulaData NONAME13=DRAWICON(IF(ONMAIN, 0, STOPSELLS), BT ,"StopSell.Gif");NONAME13.SetAttrs("Top");
			FormulaData MAXPERF= IF(ONMAIN, EMPTY, (BALANCE/LLV(L))*HHV(H)); MAXPERF.Name="MaxPerf";
			FormulaData NONAME14=IF(ONMAIN, EMPTY, BT);
			FormulaData NONAME15=IF(ONMAIN, EMPTY, MAXPERF);NONAME15.SetAttrs(" DarkGreen,Width2");
			
			return new FormulaPackage(new FormulaData[]{NONAME0,NONAME1,NONAME2,NONAME3,NONAME4,NONAME5,NONAME6,NONAME7,NONAME8,NONAME9,NONAME10,NONAME11,NONAME12,NONAME13,NONAME14,NONAME15},"");
		}
	
		public override string LongName
		{
			get{return "Fisher Transform";}
		}
	
		public override string Description
		{
			get{return "Fisher Transform osilatörü, fiyat verilerini normal dağılıma dönüştürerek trend değişikliklerini erken tespit etmek için kullanılır. John Ehlers tarafından geliştirilmiştir. +2 ve -2 arasında salınım yapar ve bu sınırlar aşıldığında güçlü sinyal verir.";}
		}
		public override string OutputFields
		{
			get{return "FISHER_LINE,TRIGGER_LINE,FISHER_TREND,OVERBOUGHT_LEVEL,0,OVERSOLD_LEVEL,DRAWICON(BUY, if(OnMain, C, BT) ,\"Buy.Gif\"),DRAWICON(SELL, if(OnMain, C, BT) ,\"Sell.Gif\"),DRAWICON(OPENSELL, if(OnMain, C, BT) ,\"OpenSell.Gif\"),DRAWICON(CLOSEOPENSELL, if(OnMain, C, BT) ,\"CloseOpenSell.Gif\"),DRAWICON(if(OnMain, 0, ALTBUY), BT ,\"AltBuy.Gif\"),DRAWICON(if(OnMain, 0, ALTSELL), BT ,\"AltSell.Gif\"),DRAWICON(if(OnMain, 0, STOPBUYS), BT ,\"StopBuy.Gif\"),DRAWICON(if(OnMain, 0, STOPSELLS), BT ,\"StopSell.Gif\"),if(OnMain, EMPTY, BT),if(OnMain, EMPTY, MaxPerf)";}
		}
	} //class FISHER

	[System.Serializable]
	public class StochRSIClaude:FormulaBase
	{
		public double RSI_N=0;
		public double STOCH_N=0;
		public double K_N=0;
		public double D_N=0;
		public double OVER_BOUGHT=0;
		public double OVER_SOLD=0;
		public StochRSIClaude():base()
		{
			AddParam("RSI_N","14","5","50","1","RSI Periyodu",FormulaParamType.Double);
			AddParam("STOCH_N","14","5","50","1","Stochastic Periyodu",FormulaParamType.Double);
			AddParam("K_N","3","1","10","1","K Hattı Düzgünleştirme",FormulaParamType.Double);
			AddParam("D_N","3","1","10","1","D Hattı Düzgünleştirme",FormulaParamType.Double);
			AddParam("OVER_BOUGHT","80","70","90","1","Aşırı Alım Seviyesi",FormulaParamType.Double);
			AddParam("OVER_SOLD","20","10","30","1","Aşırı Satım Seviyesi",FormulaParamType.Double);
		}
	
		public override FormulaPackage Run(IFormulaDataProvider DP)
		{
			this.DataProvider = DP;
			FormulaData LC= REF(CLOSE,1); LC.Name="LC";
			FormulaData RSI= SMA(MAX(CLOSE-LC,0),RSI_N,1)/SMA(ABS(CLOSE-LC),RSI_N,1)*100; RSI.Name="RSI";
			FormulaData STOCH_RAW= (RSI-LLV(RSI,STOCH_N))/(HHV(RSI,STOCH_N)-LLV(RSI,STOCH_N))*100; STOCH_RAW.Name="STOCH_RAW";
			FormulaData K= SMA(STOCH_RAW,K_N,1); K.Name="K";
			FormulaData D= SMA(K,D_N,1); D.Name="D";
			FormulaData NONAME0=OVER_BOUGHT;NONAME0.SetAttrs(" ColorRed, StyleDash");
			FormulaData NONAME1=OVER_SOLD;NONAME1.SetAttrs(" ColorGreen, StyleDash");
			FormulaData NONAME2=50;NONAME2.SetAttrs(" ColorGray, StyleDot");
			return new FormulaPackage(new FormulaData[]{K,D,NONAME0,NONAME1,NONAME2},"");
		}
	
		public override string LongName
		{
			get{return "Gelişmiş Stochastic RSI";}
		}
	
		public override string Description
		{
			get{return "Gelişmiş Stochastic RSI - K ve D çizgileri ile referans seviyeleri. RSI'ın belirli bir periyottaki min-max aralığındaki konumunu gösterir. Çift düzgünleştirme ile daha stabil sinyaller sağlar.";}
		}
		public override string OutputFields
		{
			get{return "K,D,OVER_BOUGHT,OVER_SOLD,50";}
		}
	} //class StochRSIClaude

	#endregion

} // namespace FML
