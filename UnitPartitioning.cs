using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;

namespace courtline
{
    public class UnitPartitioning
    {
        //格子ピッチデータ
        private class arytypPitch
        {
            public int lngPitch1 { get; set; }
            public int lngPitch1Su { get; set; }
            public int lngPitch2 { get; set; }
            public int lngPitch2Su { get; set; }
            public int lngDefaultSa { get; set; }
            public Boolean blnDefault { get; set; }
        }

        private class arytypHariSelect
        {
            public string Hari { get; set; }
            public int Mituke { get; set; }
            public int Mikomi { get; set; }
            public int DPitchMin { get; set; }
            public int HPitchMin { get; set; }
        }
        private const string DekisunYuusen = "出来寸優先";
        //格子ピッチ可能範囲
        private const string KanouPitch20x30 = "40,60";    //可能格子ピッチ（20×30）、下限,上限
        private const string KanouPitch30x50 = "50,90";    //可能格子ピッチ（30×50）、下限,上限
        private const string KanouPitch50x50 = "70,150";   //可能格子ピッチ（50×50）、下限,上限
        private const string KanouPitchClear = "70,150";   //可能格子ピッチ（クリア格子）、下限,上限
        private const string KanouPitchEcoRl = "50,90";    //可能格子ピッチ（エコリル格子）、下限,上限
        private const string KanouPitchLouvr = "60,60";    //可能格子ピッチ（横格子ルーバー）、下限,上限
        private const string KanouPitchYokoA = "125,125";  //可能格子ピッチ（横格子面材A）、下限,上限
        //柱関連
        private const int HasiraMitukeN = 50;              //独立用50×70柱見付
        private const int HasiraMitukeC = 70;              //独立用70×70コーナー柱見付
        private const int HasiraMikomi = 70;               //独立用50×70柱（70×70コーナー柱）見込

        private const int HasiraUmekomi = 350;             //独立用50×70柱（70×70コーナー柱）埋め込み寸法
        private const int HasiraKCheck = 56;               //独立横格子用胴縁取付材との干渉距離（両芯寸法）

        //胴縁関連
        private const int DTZaiL = 4400;                   //胴縁取付材L寸法（胴縁取付材分割寸法）
        private const int DTZaiLTMin = 1100;               //胴縁取付材L寸法（縦格子時最小寸法）
        private const double Kousa = 0.5;                  //胴縁取付材、胴縁公差
        private const int DoubutiHasiIti = 150;            //最上下、左右胴縁位置（格子端からの芯寸法）
        private const int DoubutiWait = 1000;              //胴縁重量（胴縁が何本になろうが固定値＝1Kg。胴縁=0.27kg/m×0.835×5本=1.12725kg）
        private const int DoubutiHasiItiKB = 58;           //左右胴縁位置（壁からの芯寸法）※壁内、横格子で胴縁取付材で納める場合
        private const int DoubutiHasiItiDK = 200;          //左右端胴縁位置（独立横格子のみ使用。格子端からの芯寸法）
        private const int DoubutiPMinDT = 64;              //胴縁納まり時胴縁最小ピッチ（縦格子）
        private const int DoubutiPMin56T = 116;            //56はり納まり時胴縁最小ピッチ（縦格子）
        private const int DoubutiPMin75T = 135;             //75はり納まり時胴縁最小ピッチ（縦格子）
        private const int DoubutiPMinDKT = 106;            //独立納まり時胴縁最小ピッチ（縦格子）
        private const int DoubutiPMinDY = 80;              //胴縁納まり時胴縁最小ピッチ（横格子）
        private const int DoubutiPMin56Y = 64;             //56はり納まり時胴縁最小ピッチ（横格子）
        private const int DoubutiPMin75Y = 64;             //75はり納まり時胴縁最小ピッチ（横格子）
        private const int DoubutiPMinDKY = 64;             //独立納まり時胴縁最小ピッチ（横格子）

        //'重量系
        //■格子重量(g)=(MY×単位重量(下記)×格子本数)÷1000＋胴縁重量(1000g) ※小数点以下切り捨て
        private const int MaxWait = 20000;                //最大重量（グラム＝20Kg）


        //はり関連
        private const int HariHasiIti = 150;               //最上下、左右はり位置（格子端からの芯寸法）
        private const int HariL = 4400;                    //はり定尺寸法（はり分割寸法）
        private const int HariLMin = 1100;                 //はり寸法最小
        private const int HariIdou = 28;                   //胴縁とはりの干渉距離（移動距離）

        public OutputJSON SetUnitPartitioning()
        {
            InputJSON tIn = JSONSet();
            OutputJSON tOut = SC02_UnitSeparation(tIn);
            tOut = SC03_HariCalc(tIn, tOut);
            tOut = SC04_HasiraCalc(tIn, tOut);
            return tOut;
        }
        //ユニット分割処理
        private OutputJSON SC02_UnitSeparation(InputJSON tIn)
        {
            OutputJSON OutputJSON = new OutputJSON();
            OutputJSON.Result = new Result();
            OutputJSON.tOut = new tOut();
            OutputJSON.aryUnits = new List<aryUnit>();
            OutputJSON.aryHaris = new List<aryHari>();
            OutputJSON.aryHariLs = new List<aryHariL>();
            OutputJSON.aryDTZaiLs = new List<aryDTZaiL>();
            OutputJSON.aryDTZais = new List<aryDTZai>();
            OutputJSON.aryHasiras = new List<aryHasira>();
            List<arytypPitch> arytypPitchs;
            List<aryUnit> aryUnitYWk;        //横方向ユニット分割結果ワーク
            int lngUnitYWk;                  //横方向ユニット分割結果数
            List<aryUnit> aryUnitWk;         //ユニット分割試行用ワーク
            int lngUnitWk;                   //ユニット分割試行用数
            int lngUnitKousiSu;              //ユニット内格子本数（縦格子時）
            int lngTotalUnitSu;              //ユニット数合計（縦格子時）
            int lngKousiW;                   //横方向格子Ｗ（縦格子時）
            int lngP1Su;                     //ユニット内ピッチ1格子本数（縦格子時）
            int lngP2Su;                     //ユニット内ピッチ2格子本数（縦格子時）
            int lngWidth;                    //計算用格子見付（縦格子用）
            int lngLength;                   //計算用Ｗ（縦格子用）
            int lngLengthSum;                //計算用合計Ｗ（縦格子用）
            int lngKousiLen;                 //格子寸法（縦格子用）
            int lngWaritukeW;                //横方向割付範囲Ｗ
            int lngWUnitSu;                  //横方向分割数
            int lngHUnitSu;                  //縦方向分割数
            int lngKLenW;                    //格子長さ（調整前）
            int lngKHonsu;                   //格子本数※1ユニット当り
            int lngKHonsuHD;                 //格子本数※各段別
            int lngKPitchsu;                 //格子ピッチ数※各段別
            int lngKAmari;                   //格子余り本数
            int lngAmari;                    //余り寸法
            int lngKAmariC;                  //格子余り本数（計算用、縦格子用）
            int lngHAmari;                   //余り寸法（縦方向、縦格子用）
            int lngSa;                       //計算誤差
            int lngKPitch1;                  //格子ピッチ1
            int lngKPitch1Su;                //格子ピッチ1数
            int lngKPitch1ZSu;               //格子ピッチ1残数
            int lngKPitch2;                  //格子ピッチ2
            int lngKPitch2Su;                //格子ピッチ2数
            int lngKPitch2ZSu;               //格子ピッチ2残数
            int lngKPitchSuWk;               //格子ピッチ数計算ワーク
            int lngDLen;                     //胴縁長（公差抜き）
            int lngDPitchK;                  //胴縁ピッチ
            int lngDAmari;                   //胴縁位置余り寸法
            int lngDPitchMin = 0;            //胴縁最小ピッチ
            int lngHPitchMin = 0;            //はり最小ピッチ
            //◎◎
            int lngDHasiItiL;                //胴縁左端位置（横格子の場合）
            int lngDHasiItiR;                //胴縁右端位置（横格子の場合）

            string strHari = "";             //はり種類
            int lngHMituke = 0;              //はり見付
            int lngHMikomi = 0;              //はり見込
            int lngKousa;                    //胴縁公差
            int lngHeight;                   //トータル高さ
            Boolean blnMaxOver;              //上限超えフラグ（再分割）
            Boolean blnKHSuUnder;            //格子本数下限未満フラグ（再分割）
            int lngKHSuUnder;                //格子本数下限未満ループ回数
            int lngReSize=0;                 //再計算結果寸法
            List<arytypPitch> aryPitch= new List<arytypPitch>();//ピッチ候補計算インターフェース
            int i;
            int j;
            int k = 0;
            int l;
            int m;
            int n;
            int o;
            int p;
            if (tIn.Osamari == "壁内" || tIn.Osamari == "持出し" || tIn.Osamari == "独立")
            {
                switch (tIn.TateYoko)
                {
                    case "縦格子":
                        //①格子割付（横方向）
                        if (tIn.KPitchText == "")
                        {
                            if (tIn.YMode == DekisunYuusen)
                            {
                                //出来寸優先の場合
                                //格子W計算
                                //☆☆
                                if (tIn.Osamari == "独立")
                                {
                                    //●コーナー格子使用時は出来寸優先でもa,b寸法が発生する
                                    lngKousiW = tIn.l + tIn.c + tIn.d - tIn.a - tIn.b;
                                }
                                else
                                {
                                    lngKousiW = tIn.l - tIn.a - tIn.b;
                                }
                                //入力内容確定ボタン実行の場合
                                //ピッチバリエーション取得
                                arytypPitchs = GetPitchCalc(tIn.Kousi, tIn.Mituke, lngKousiW, tIn.Kpitch, aryPitch);
                                //候補採用優先順位
                                //1：ピッチ1が画面入力ピッチと同じでピッチ2が使われない（blnDefaultがTrue）
                                //2：ピッチ数の差が最小のもの（blnDefaultがTrue）
                                //3：ピッチ1が画面入力ピッチと同じものが含まれている
                                //4：ピッチ1が画面入力ピッチの直近上位
                                j = 0;
                                for (i = 0; i < arytypPitchs.Count; i++)
                                {
                                    if (arytypPitchs[i].blnDefault)
                                    {
                                        j = i;
                                        break;
                                    }
                                    if (arytypPitchs[i].lngPitch1 >= tIn.Kpitch && j == 0) {
                                        j = i;
                                    }
                                }
                                //各ピッチ設定
                                lngKPitch1 = arytypPitchs[j].lngPitch1;
                                lngKPitch2 = arytypPitchs[j].lngPitch2;
                                //各ピッチ数設定
                                lngKPitch1Su = arytypPitchs[j].lngPitch1Su;
                                lngKPitch2Su = arytypPitchs[j].lngPitch2Su;
                                //ピッチ数計算
                                lngKPitchsu = lngKPitch1Su + lngKPitch2Su;
                                //格子本数計算
                                lngKHonsu = lngKPitchsu + 1;
                                //TOut設定
                                OutputJSON.tOut.H = tIn.H;
                                OutputJSON.tOut.a = tIn.a;
                                OutputJSON.tOut.b = tIn.b;
                                OutputJSON.tOut.c = tIn.c;
                                OutputJSON.tOut.d = tIn.d;
                                OutputJSON.tOut.HHonsu = 0;
                                OutputJSON.tOut.KHonsu = lngKHonsu;
                                OutputJSON.tOut.KPitch1 = lngKPitch1;
                                OutputJSON.tOut.KPitch1Su = lngKPitch1Su;
                                OutputJSON.tOut.KPitch2 = lngKPitch2;
                                OutputJSON.tOut.KPitch2Su = lngKPitch2Su;
                                OutputJSON.tOut.KPitchSu = OutputJSON.tOut.KPitch1Su + OutputJSON.tOut.KPitch2Su;
                                OutputJSON.tOut.Sa = 0;
                                //☆☆
                                if (tIn.Osamari == "独立")
                                {
                                    //Wにはa,bを加味してはいけない
                                    OutputJSON.tOut.W = tIn.l + tIn.c + tIn.d;
                                }
                                else
                                {
                                    OutputJSON.tOut.W = tIn.l - tIn.c - tIn.d;
                                }
                                OutputJSON.tOut.ReCalcCnt1 = 0;
                                OutputJSON.tOut.ReCalcCnt2 = 0;
                                OutputJSON.tOut.ReCalcCnt3 = 0;
                                //☆☆
                                OutputJSON.tOut.SBSukima = tIn.SBSukima;
                            }
                            else
                            {
                                //ピッチ優先の場合
                                //割付範囲計算
                                //☆☆
                                if (tIn.Osamari == "独立")
                                {
                                    lngWaritukeW = tIn.l + tIn.c + tIn.d - tIn.Mituke;
                                }
                                else
                                {
                                    lngWaritukeW = tIn.l - tIn.a - tIn.b - tIn.Mituke;
                                }
                                //ピッチ数計算
                                lngKPitchsu = RoundDown(lngWaritukeW / tIn.Kpitch, 0);
                                //格子本数計算
                                lngKHonsu = lngKPitchsu + 1;
                                //差計算
                                lngSa = lngWaritukeW - (lngKPitchsu * tIn.Kpitch);
                                //格子W計算
                                //☆☆
                                if (tIn.Osamari == "独立")
                                {
                                    lngKousiW = tIn.l + tIn.c + tIn.d - lngSa;
                                }
                                else
                                {
                                    lngKousiW = tIn.l - tIn.a - tIn.b - lngSa;
                                }
                                //各ピッチ数設定
                                lngKPitch1Su = lngKPitchsu;
                                lngKPitch2Su = 0;
                                //各ピッチ設定
                                lngKPitch1 = tIn.Kpitch;
                                lngKPitch2 = 0;
                                //TOut設定
                                OutputJSON.tOut.H = tIn.H;
                                //☆☆
                                if (tIn.Osamari == "独立")
                                {
                                    OutputJSON.tOut.a = RoundDown(lngSa / 2, 0);
                                    OutputJSON.tOut.b = lngSa - RoundDown(lngSa / 2, 0);
                                    OutputJSON.tOut.c = tIn.c;
                                    OutputJSON.tOut.d = tIn.d;
                                }
                                else
                                {
                                    OutputJSON.tOut.a = tIn.a + RoundDown(lngSa / 2, 0);
                                    OutputJSON.tOut.b = tIn.b + (lngSa - RoundDown(lngSa / 2, 0));
                                    OutputJSON.tOut.c = tIn.c;
                                    OutputJSON.tOut.d = tIn.d;
                                }
                                OutputJSON.tOut.HHonsu = 0;
                                OutputJSON.tOut.KHonsu = lngKHonsu;
                                OutputJSON.tOut.KPitch1 = lngKPitch1;
                                OutputJSON.tOut.KPitch1Su = lngKPitch1Su;
                                OutputJSON.tOut.KPitch2 = lngKPitch2;
                                OutputJSON.tOut.KPitch2Su = lngKPitch2Su;
                                OutputJSON.tOut.KPitchSu = OutputJSON.tOut.KPitch1Su + OutputJSON.tOut.KPitch2Su;
                                OutputJSON.tOut.Sa = lngSa;
                                //☆☆
                                if (tIn.Osamari == "独立")
                                {
                                    //胴縁寸法はあくまでも基準面寸法。ピッチ優先の格子Wは差を吸収するとその分短くなるが
                                    //胴縁寸法は入力寸法そのまま
                                    OutputJSON.tOut.W = tIn.l + tIn.c + tIn.d;
                                }
                                else
                                {
                                    OutputJSON.tOut.W = tIn.l - tIn.c - tIn.d;
                                }
                                OutputJSON.tOut.ReCalcCnt1 = 0;
                                OutputJSON.tOut.ReCalcCnt2 = 0;
                                OutputJSON.tOut.ReCalcCnt3 = 0;
                                //☆☆
                                OutputJSON.tOut.SBSukima = tIn.SBSukima;
                            }
                        }
                        else
                        {
                            //ピッチ選択画面からの戻り（出来寸優先のみ）
                            //☆☆
                            if (tIn.Osamari == "独立")
                            {
                                //●コーナー格子使用時は出来寸優先でもa,b寸法が発生する
                                lngKousiW = tIn.l + tIn.c + tIn.d - tIn.a - tIn.b;
                            }
                            else
                            {
                                lngKousiW = tIn.l - tIn.a - tIn.b;
                            }
                            i = Strings.InStr(tIn.KPitchText, "×");
                            j = Strings.InStr(tIn.KPitchText, "／");
                            if (i > 0)
                            {
                                k = Strings.InStr(i + 1, tIn.KPitchText, "×");
                            }
                            //各ピッチ設定
                            lngKPitch1 = int.Parse(tIn.KPitchText.Substring(1, i - 1));
                            if (k > 0)
                            {
                                lngKPitch2 = int.Parse(tIn.KPitchText.Substring(j + 1, k - j - 1));
                            }
                            else
                            {
                                lngKPitch2 = -1; //マイナスはピッチ２が使われていないことを示す
                            }
                            //各ピッチ数設定
                            if (k > 0)
                            {
                                lngKPitch1Su = int.Parse(tIn.KPitchText.Substring(i + 1, j - i - 1));
                                lngKPitch2Su = int.Parse(tIn.KPitchText.Substring(k + 1));
                            }
                            else
                            {
                                lngKPitch1Su = int.Parse(tIn.KPitchText.Substring(i + 1));
                                lngKPitch2Su = 0;
                            }
                            //ピッチ数計算
                            lngKPitchsu = lngKPitch1Su + lngKPitch2Su;
                            //格子本数計算
                            lngKHonsu = lngKPitchsu + 1;
                            //TOut設定
                            OutputJSON.tOut.H = tIn.H;
                            OutputJSON.tOut.a = tIn.a;
                            OutputJSON.tOut.b = tIn.b;
                            OutputJSON.tOut.c = tIn.c;
                            OutputJSON.tOut.d = tIn.d;
                            OutputJSON.tOut.HHonsu = 0;
                            OutputJSON.tOut.KHonsu = lngKHonsu;
                            OutputJSON.tOut.KPitch1 = lngKPitch1;
                            OutputJSON.tOut.KPitch1Su = lngKPitch1Su;
                            OutputJSON.tOut.KPitch2 = lngKPitch2;
                            OutputJSON.tOut.KPitch2Su = lngKPitch2Su;
                            OutputJSON.tOut.KPitchSu = OutputJSON.tOut.KPitch1Su + OutputJSON.tOut.KPitch2Su;
                            OutputJSON.tOut.Sa = 0;
                            //☆☆
                            if (tIn.Osamari == "独立")
                            {
                                //Wにはa,bを加味してはいけない
                                OutputJSON.tOut.W = tIn.l + tIn.c + tIn.d;
                            }
                            else
                            {
                                OutputJSON.tOut.W = tIn.l - tIn.c - tIn.d;
                            }
                            OutputJSON.tOut.ReCalcCnt1 = 0;
                            OutputJSON.tOut.ReCalcCnt2 = 0;
                            OutputJSON.tOut.ReCalcCnt3 = 0;
                            //☆☆
                            OutputJSON.tOut.SBSukima = tIn.SBSukima;
                        }
                        //②横方向ユニット分割
                        lngKHSuUnder = 0;                        //格子本数下限未満ループ回数
                        //ユニット数
                        lngWUnitSu = (int)RoundUp((double)lngKousiW / (double)tIn.MaxW, 0);
ReSeparateKT:
                        //ユニット格子数
                        lngUnitKousiSu = RoundDown(lngKHonsu / lngWUnitSu, 0);
                        //格子余り（左ユニットから1本ずつ割り当て）
                        lngKAmari = lngKHonsu - (lngWUnitSu * lngUnitKousiSu);
                        lngKAmariC = lngKAmari;
                        //胴縁公差
                        lngKousa = GetDoubutiKousa(tIn, OutputJSON, OutputJSON.tOut.W, lngWUnitSu);
                        j = 0;                                           //ユニット内格子本数
                        lngP1Su = 0;                                     //ユニット内ピッチ1格子本数
                        lngP2Su = 0;                                    //ユニット内ピッチ2格子本数
                        lngWidth = tIn.Mituke;                           //格子見付を初期値（最初の格子の見付半分＋最後の格子の見付半分）
                        //格子面から胴縁がはみ出す部分を追加する（左端ユニットのみ）
                        //☆☆
                        if (tIn.Osamari == "独立")
                        {
                            //独立縦格子の場合、ピッチ優先でも胴縁端は持出し寸法のまま
                            lngWidth = lngWidth + OutputJSON.tOut.a;
                        }
                        else
                        {
                            if (OutputJSON.tOut.a != tIn.c)
                            {
                                //a寸法とｃ寸法の値が異なる場合、その差を追加する
                                lngWidth = lngWidth + OutputJSON.tOut.a - tIn.c;                        //ユニット
                            }
                        }
                        lngLengthSum = 0;                               //トータルＷ
                        k = 0;                                          //ユニット数
                        blnMaxOver = false;                             //ユニットMAXオーバーチェック用
                        blnKHSuUnder = false;                           //格子本数下限未満チェック用
                        lngKHSuUnder = 0;                               //格子本数下限未満ループ回数
                        //ユニットデータ作成（横）
                        for (i = 1; i <= lngKPitch1Su + lngKPitch2Su; i++)
                        {
                            j = j + 1;
                            if (i > lngKPitch1Su)
                            {
                                lngLength = lngKPitch2;
                                lngP2Su = lngP2Su + 1;
                            }
                            else
                            {
                                lngLength = lngKPitch1;
                                lngP1Su = lngP1Su + 1;
                            }
                            //ユニットW（胴縁長)※lngWidthは集計項目
                            //Debug.Print "i:" & i & "/lngWidth:" & lngWidth & "/lngLength:" & lngLength
                            lngWidth = lngWidth + lngLength;
                            if ((i == lngKPitch1Su + lngKPitch2Su) ||
                                (lngKAmariC == 0 && j == lngUnitKousiSu - 1 && k == 0) ||
                                (lngKAmariC == 0 && j == lngUnitKousiSu) ||
                                (lngKAmariC > 0 && j == lngUnitKousiSu && k == 0) ||
                                (lngKAmariC > 0 && j > lngUnitKousiSu)) {
                                //ユニット情報作成
                                //格子余りがない場合：ユニット格子数に達した時
                                //格子余りがある場合：余り格子を割り当てるユニットの時
                                if (lngKAmariC > 0)
                                {
                                    //格子余りを減算
                                    lngKAmariC = lngKAmariC - 1;
                                }
                                k = k + 1;
                                aryUnit aryUnit = new aryUnit();
                                aryUnit.No = k;                               //ユニットNo
                                if (k == 1)
                                {
                                    aryUnit.Honsu = j + 1;                    //格子本数（先頭ユニットだけは＋1）
                                }
                                else
                                {
                                    aryUnit.Honsu = j;                        //格子本数
                                }
                                aryUnit.Kousa = 0;                            //公差
                                if (lngKousa > 0 && (k > lngWUnitSu - lngKousa)) {
                                    if (tIn.Kousi == "クリア格子")
                                    {
                                        if (lngWUnitSu == 2)
                                        {
                                            if (k == 2)
                                            {
                                                aryUnit.Kousa = lngKousa;     //２分割のクリア格子のみ２ユニット目に2mm公差
                                            }
                                        }
                                        else
                                        {
                                            aryUnit.Kousa = 1;                //公差
                                        }
                                    }
                                    else
                                    {
                                        aryUnit.Kousa = 1;                    //公差
                                    }
                                }
                                aryUnit.Pitch1Su = lngP1Su;                   //Pitch1の数
                                if (lngP1Su > 0)
                                {
                                    aryUnit.Pitch1 = lngKPitch1;              //Pitch1
                                }
                                aryUnit.Pitch2Su = lngP2Su;                   //Pitch2の数
                                if (lngP2Su > 0)
                                {
                                    aryUnit.Pitch2 = lngKPitch2;              //Pitch2
                                }
                                if (k == lngWUnitSu)
                                {
                                    if (tIn.Osamari == "独立")
                                    {
                                        //独立縦格子の場合、格子面から胴縁がはみ出す分を追加
                                        aryUnit.Width = lngWidth - aryUnit.Kousa + OutputJSON.tOut.b;
                                    }
                                    else
                                    {
                                        //最終ユニットの場合は胴縁がはみ出る分を足す
                                        aryUnit.Width = lngWidth - aryUnit.Kousa + OutputJSON.tOut.b - tIn.d;                  //ユニットの幅
                                    }
                                }
                                else
                                {
                                    aryUnit.Width = lngWidth - aryUnit.Kousa;        //ユニットの幅
                                }
                                if (aryUnit.Width + aryUnit.Kousa > tIn.MaxW)
                                {
                                    //幅（ここでは公差を含んだ値）がMaxオーバーの場合
                                    OutputJSON.tOut.ReCalcCnt1 = OutputJSON.tOut.ReCalcCnt1 + 1;
                                    blnMaxOver = true;
                                }
                                lngLengthSum = lngLengthSum + aryUnit.Width + aryUnit.Kousa;                   //トータルＷ
                                //リセット
                                j = 0;                                      //ユニット内格子本数
                                lngP1Su = 0;                                //ユニット内Pitch1適用数
                                lngP2Su = 0;                                //ユニット内Pitch2適用数
                                lngWidth = 0;                               //２ユニットからは初期値
                                if(k <= OutputJSON.aryUnits.Count)
                                {
                                    OutputJSON.aryUnits[k - 1].No = aryUnit.No;
                                    OutputJSON.aryUnits[k - 1].Pitch1 = aryUnit.Pitch1;
                                    OutputJSON.aryUnits[k - 1].Pitch1Su = aryUnit.Pitch1Su;
                                    OutputJSON.aryUnits[k - 1].Pitch2 = aryUnit.Pitch2;
                                    OutputJSON.aryUnits[k - 1].Pitch2Su = aryUnit.Pitch2Su;
                                    OutputJSON.aryUnits[k - 1].Honsu = aryUnit.Honsu;
                                    OutputJSON.aryUnits[k - 1].Width = aryUnit.Width;
                                    OutputJSON.aryUnits[k - 1].Kousa = aryUnit.Kousa;
                                    OutputJSON.aryUnits[k - 1].Sx = aryUnit.Sx;
                                    OutputJSON.aryUnits[k - 1].Ex = aryUnit.Ex;
                                    OutputJSON.aryUnits[k - 1].Height = aryUnit.Height;
                                    OutputJSON.aryUnits[k - 1].Sy = aryUnit.Sy;
                                    OutputJSON.aryUnits[k - 1].Ey = aryUnit.Ey;
                                    OutputJSON.aryUnits[k - 1].DoubutiSu = aryUnit.DoubutiSu;
                                    OutputJSON.aryUnits[k - 1].MX = aryUnit.MX;
                                    OutputJSON.aryUnits[k - 1].ZT = aryUnit.ZT;
                                    OutputJSON.aryUnits[k - 1].ZM = aryUnit.ZM;
                                    OutputJSON.aryUnits[k - 1].KA = aryUnit.KA;
                                    OutputJSON.aryUnits[k - 1].ZN = aryUnit.ZN;
                                    OutputJSON.aryUnits[k - 1].KB = aryUnit.KB;
                                    OutputJSON.aryUnits[k - 1].YT = aryUnit.YT;
                                    OutputJSON.aryUnits[k - 1].MY = aryUnit.MY;
                                    OutputJSON.aryUnits[k - 1].ZS = aryUnit.ZS;
                                    OutputJSON.aryUnits[k - 1].ZF = aryUnit.ZF;
                                    OutputJSON.aryUnits[k - 1].ZG = aryUnit.ZG;
                                    OutputJSON.aryUnits[k - 1].ZI = aryUnit.ZI;
                                    OutputJSON.aryUnits[k - 1].ZL = aryUnit.ZL;
                                    OutputJSON.aryUnits[k - 1].YS = aryUnit.YS;
                                    OutputJSON.aryUnits[k - 1].Wait = aryUnit.Wait;

                                }
                                else
                                {
                                    OutputJSON.aryUnits.Add(aryUnit);
                                }
                                
                            }
                        }

                        //幅がMaxオーバーの場合、ユニット数を＋1して再計算
                        if (blnMaxOver)
                        {
                            lngWUnitSu = lngWUnitSu + 1;
                            goto ReSeparateKT;
                        }
                        //③縦方向ユニット分割
                        //☆☆
                        if (tIn.Osamari == "独立")
                        {
                            //独立はHMax=2800であり、希望Hも指定しないので縦分割されない
                            lngHUnitSu = 1;
                            lngKousiLen = tIn.H - tIn.SBSukima;
                            lngHAmari = 0;
                        }
                        else
                        {
                            lngHUnitSu = (int)RoundUp((double)tIn.H / (double)tIn.MaxH, 0);
                            lngKousiLen = RoundDown((tIn.H - (tIn.TSukima * (lngHUnitSu - 1))) / lngHUnitSu, 0);
                            lngHAmari = tIn.H - (tIn.TSukima * (lngHUnitSu - 1)) - (lngKousiLen * lngHUnitSu);
                        }
                        //ユニットデータ作成（縦）
                        lngTotalUnitSu = lngWUnitSu;
                        //一番上は横分割結果をそのまま使用する
                        for (i = 1; i <=  lngHUnitSu; i++)
                        {
                            for (j = 1; j <= lngWUnitSu; j++)
                            {
                                //横ループ
                                if (i > 1)
                                {
                                    //縦2分割以上はユニットを作成
                                    lngTotalUnitSu = lngTotalUnitSu + 1;
                                    //1段上のユニット情報の横方向情報をコピー
                                    if (i > 1)
                                    {
                                        k = (i - 2) * lngWUnitSu + j;
                                    }
                                    else
                                    {
                                        k = (i - 1) * lngWUnitSu + j;
                                    }
                                    aryUnit aryUnit = new aryUnit();
                                    aryUnit.No = lngTotalUnitSu;
                                    aryUnit.Pitch1 = OutputJSON.aryUnits[k-1].Pitch1;
                                    aryUnit.Pitch1Su = OutputJSON.aryUnits[k - 1].Pitch1Su;
                                    aryUnit.Pitch2 = OutputJSON.aryUnits[k - 1].Pitch2;
                                    aryUnit.Pitch2Su = OutputJSON.aryUnits[k - 1].Pitch2Su;
                                    aryUnit.Width = OutputJSON.aryUnits[k - 1].Width;
                                    aryUnit.Honsu = OutputJSON.aryUnits[k - 1].Honsu;
                                    aryUnit.Kousa = OutputJSON.aryUnits[k - 1].Kousa;
                                    aryUnit.Sx = OutputJSON.aryUnits[k - 1].Sx;
                                    aryUnit.Ex = OutputJSON.aryUnits[k - 1].Ex;
                                    OutputJSON.aryUnits.Add(aryUnit);
                                }
                            }
                        }
                        //縦情報を含む詳細設定（全ユニットへ）
                        for (i = 1; i <= lngHUnitSu; i++)
                        {
                            for (j = 1; j <= lngWUnitSu; j++)
                            {
                                k = (i - 1) * lngWUnitSu + j;
                                //H情報を設定
                                //格子長
                                if (lngHAmari > 0 && (i > lngHUnitSu - lngHAmari)) {
                                    OutputJSON.aryUnits[k-1].Height = lngKousiLen + 1;
                                }
                                else
                                {
                                    OutputJSON.aryUnits[k-1].Height = lngKousiLen + 0;
                                }
                                //格子長（MY)
                                OutputJSON.aryUnits[k-1].MY = OutputJSON.aryUnits[k-1].Height;
                                //1段上のユニットIDX
                                if (i > 1)
                                {
                                    l = (i - 2) * lngWUnitSu + j;
                                }
                                else
                                {
                                    l = (i - 1) * lngWUnitSu + j;
                                }
                                //胴縁本数
                                if (tIn.Kousi == "３０×５０格子" || tIn.Kousi == "５０×５０格子")
                                {
                                    OutputJSON.aryUnits[k-1].DoubutiSu = 2;
                                    if (3000 < OutputJSON.aryUnits[k-1].Height && OutputJSON.aryUnits[k-1].Height <= 4000)
                                    {
                                        OutputJSON.aryUnits[k-1].DoubutiSu = OutputJSON.aryUnits[k-1].DoubutiSu + 1;
                                    }
                                    else if (4000 < OutputJSON.aryUnits[k - 1].Height && OutputJSON.aryUnits[k - 1].Height <= 5000)
                                    {
                                        OutputJSON.aryUnits[k-1].DoubutiSu = OutputJSON.aryUnits[k-1].DoubutiSu + 2;
                                    }
                                }
                                else
                                {
                                    OutputJSON.aryUnits[k-1].DoubutiSu = 2;
                                    if (2000 < OutputJSON.aryUnits[k-1].Height && OutputJSON.aryUnits[k-1].Height <= 3000)
                                    {
                                        OutputJSON.aryUnits[k-1].DoubutiSu = OutputJSON.aryUnits[k-1].DoubutiSu + 1;
                                    } else if (3000 < OutputJSON.aryUnits[k-1].Height && OutputJSON.aryUnits[k-1].Height <= 4000)
                                    {
                                        OutputJSON.aryUnits[k-1].DoubutiSu = OutputJSON.aryUnits[k-1].DoubutiSu + 2;
                                    } else if (4000 < OutputJSON.aryUnits[k-1].Height && OutputJSON.aryUnits[k-1].Height <= 5000)
                                    {
                                        OutputJSON.aryUnits[k-1].DoubutiSu = OutputJSON.aryUnits[k-1].DoubutiSu + 3;
                                    }
                                }
                                //胴縁ピッチ
                                //はりを決定
                                arytypHariSelect arytypHariSelectWK= HariSelect(tIn);
                                strHari = arytypHariSelectWK.Hari;
                                lngHMituke = arytypHariSelectWK.Mituke;
                                lngHMikomi = arytypHariSelectWK.Mikomi; 
                                lngDPitchMin = arytypHariSelectWK.DPitchMin; 
                                lngHPitchMin = arytypHariSelectWK.HPitchMin;

                                if (OutputJSON.aryUnits[k-1].DoubutiSu + 1 * lngDPitchMin <= OutputJSON.aryUnits[k-1].MY && OutputJSON.aryUnits[k-1].MY < (OutputJSON.aryUnits[k-1].DoubutiSu - 1) * 150 + 300){
                                    //小さいケース①：【（胴縁本数+1)×胴縁最小ピッチ　≦　格子長さMY　＜（胴縁本数-1)×150+300の場合】
                                    lngDPitchK = RoundDown(OutputJSON.aryUnits[k-1].MY / (OutputJSON.aryUnits[k - 1].DoubutiSu + 1), 0);
                                    lngDAmari = OutputJSON.aryUnits[k - 1].MY - (lngDPitchK * (OutputJSON.aryUnits[k - 1].DoubutiSu + 1));
                                    OutputJSON.aryUnits[k - 1].ZF = -1;    //胴縁ピッチ1：-1は表示しない（値なし）の意味
                                    OutputJSON.aryUnits[k - 1].ZG = -1;    //胴縁ピッチ2：-1は表示しない（値なし）の意味
                                    OutputJSON.aryUnits[k - 1].ZI = -1;    //胴縁ピッチ3：-1は表示しない（値なし）の意味
                                    OutputJSON.aryUnits[k - 1].ZL = -1;    //胴縁ピッチ4：-1は表示しない（値なし）の意味
                                    if(lngDAmari > 0)
                                    {
                                        OutputJSON.aryUnits[k - 1].ZS = lngDPitchK + 1;
                                    }
                                    else
                                    {
                                        OutputJSON.aryUnits[k-1].ZS = lngDPitchK + 0;
                                    }
                                    OutputJSON.aryUnits[k-1].YS = lngDPitchK;
                                    //胴縁ピッチ1
                                    if (lngDAmari > 1)
                                    {
                                        OutputJSON.aryUnits[k-1].ZF = lngDPitchK + 1;
                                    }
                                    else
                                    {
                                        OutputJSON.aryUnits[k-1].ZF = lngDPitchK + 0;
                                    }
                                }else if(OutputJSON.aryUnits[k-1].MY < ((OutputJSON.aryUnits[k-1].DoubutiSu + 1) * lngDPitchMin)){
                                    //小さいケース②：【格子長さMY　＜（胴縁本数+1)×胴縁最小ピッチの場合】
                                    lngDPitchK = lngDPitchMin;
                                    OutputJSON.aryUnits[k-1].ZF = -1;    //胴縁ピッチ1：-1は表示しない（値なし）の意味
                                    OutputJSON.aryUnits[k-1].ZG = -1;    //胴縁ピッチ2：-1は表示しない（値なし）の意味
                                    OutputJSON.aryUnits[k-1].ZI = -1;    //胴縁ピッチ3：-1は表示しない（値なし）の意味
                                    OutputJSON.aryUnits[k-1].ZL = -1;    //胴縁ピッチ4：-1は表示しない（値なし）の意味
                                    //☆☆
                                    if (tIn.Osamari == "独立" && tIn.Kousi == "クリア格子" && OutputJSON.aryUnits[k-1].MY <= 191)
                                    {
                                        //独立納まりクリア格子のMY191以下の場合
                                        OutputJSON.aryUnits[k-1].ZS = OutputJSON.aryUnits[k-1].MY - (43 + lngDPitchK);
                                        OutputJSON.aryUnits[k-1].YS = 43;
                                        OutputJSON.aryUnits[k-1].ZF = lngDPitchK;
                                    }
                                    else
                                    {
                                        OutputJSON.aryUnits[k-1].ZS = (int)RoundUp((double)(OutputJSON.aryUnits[k-1].MY - lngDPitchK) / 2, 0);
                                        OutputJSON.aryUnits[k-1].YS = RoundDown((OutputJSON.aryUnits[k-1].MY - lngDPitchK) / 2, 0);
                                        OutputJSON.aryUnits[k-1].ZF = lngDPitchK;    //胴縁ピッチ1
                                    }
                                }
                                else
                                {
                                    //通常ケース
                                    lngDPitchK = RoundDown((OutputJSON.aryUnits[k-1].Height - (DoubutiHasiIti * 2)) / (OutputJSON.aryUnits[k-1].DoubutiSu - 1), 0);
                                    lngDAmari = (OutputJSON.aryUnits[k-1].Height - (DoubutiHasiIti * 2)) - (lngDPitchK * (OutputJSON.aryUnits[k-1].DoubutiSu - 1));
                                    o = lngDAmari;
                                    OutputJSON.aryUnits[k-1].ZF = -1;    //胴縁ピッチ1：-1は表示しない（値なし）の意味
                                    OutputJSON.aryUnits[k-1].ZG = -1;    //胴縁ピッチ2：-1は表示しない（値なし）の意味
                                    OutputJSON.aryUnits[k-1].ZI = -1;    //胴縁ピッチ3：-1は表示しない（値なし）の意味
                                    OutputJSON.aryUnits[k-1].ZL = -1;    //胴縁ピッチ4：-1は表示しない（値なし）の意味
                                    for (m = 1; m <= OutputJSON.aryUnits[k-1].DoubutiSu - 1;m++)
                                    {
                                        //余りの配分（胴縁ピッチの下から1mmずつ配分）
                                        if(o > 0)
                                        {
                                            p = 1;
                                            o = o - 1;
                                        }
                                        else
                                        {
                                            p = 0;
                                        }
                                        switch (m)
                                        {
                                            case 1:
                                                OutputJSON.aryUnits[k-1].ZF = lngDPitchK + p;    //胴縁ピッチ1（一番下）
                                                break;
                                            case 2:
                                                OutputJSON.aryUnits[k-1].ZG = lngDPitchK + p;    //胴縁ピッチ2    ↓
                                                break;
                                            case 3:
                                                OutputJSON.aryUnits[k-1].ZI = lngDPitchK + p;    //胴縁ピッチ3    ↓
                                                break;
                                            case 4:
                                                OutputJSON.aryUnits[k-1].ZL = lngDPitchK + p;    //胴縁ピッチ4（一番上）
                                                break;
                                            default:
                                                break;
                                        }
                                    }
                                    //上はね出し
                                    switch (i)
                                    {
                                        case 1:
                                            OutputJSON.aryUnits[k-1].YS = DoubutiHasiIti;
                                            break;
                                        default:
                                            OutputJSON.aryUnits[k-1].YS = DoubutiHasiIti;
                                            break;
                                    }
                                    //下はね出し
                                    switch (i)
                                    {
                                        case 1:
                                            OutputJSON.aryUnits[k-1].ZS = DoubutiHasiIti;
                                            break;
                                        default:
                                            OutputJSON.aryUnits[k-1].ZS = DoubutiHasiIti;
                                            break;
                                    }
                                }
                                //胴縁長
                                OutputJSON.aryUnits[k-1].MX = OutputJSON.aryUnits[k-1].Width;
                                //Widthは公差を含めないのでここで公差を追加する
                                OutputJSON.aryUnits[k-1].Width = OutputJSON.aryUnits[k-1].Width + OutputJSON.aryUnits[k-1].Kousa;
                                //格子ピッチ、格子本数
                                if(OutputJSON.aryUnits[k-1].Pitch1 == 0)
                                {
                                    if (OutputJSON.aryUnits[k-1].Pitch2 > 0)
                                    {
                                        OutputJSON.aryUnits[k-1].ZM = OutputJSON.aryUnits[k-1].Pitch2;
                                    }
                                    else
                                    {
                                        OutputJSON.aryUnits[k-1].ZM = -1;    //-1は表示しない（値なし）の意味
                                    }
                                }
                                else
                                {
                                    OutputJSON.aryUnits[k-1].ZM = OutputJSON.aryUnits[k-1].Pitch1;
                                }
                                if (OutputJSON.aryUnits[k-1].Pitch1 == 0)
                                {
                                    if (OutputJSON.aryUnits[k-1].Pitch2 > 0)
                                    {
                                        OutputJSON.aryUnits[k-1].KA = OutputJSON.aryUnits[k-1].Pitch2Su;
                                    }
                                    else
                                    {
                                        OutputJSON.aryUnits[k-1].KA = -1;    //-1は表示しない（値なし）の意味
                                    }
                                }
                                else
                                {
                                    if(j == 1)
                                    {
                                        //左端のユニット
                                        OutputJSON.aryUnits[k-1].KA = OutputJSON.aryUnits[k-1].Pitch1Su + 1; //格子本数

                                    }
                                    else
                                    {
                                        OutputJSON.aryUnits[k-1].KA = OutputJSON.aryUnits[k-1].Pitch1Su;
                                    }
                                }
                                if (OutputJSON.aryUnits[k-1].Pitch1Su == 0 || OutputJSON.aryUnits[k-1].Pitch2Su == 0)
                                {
                                    OutputJSON.aryUnits[k-1].ZN = -1;    //-1は表示しない（値なし）の意味
                                    OutputJSON.aryUnits[k-1].KB = -1;    //-1は表示しない（値なし）の意味
                                }
                                else
                                {
                                    OutputJSON.aryUnits[k-1].ZN = OutputJSON.aryUnits[k-1].Pitch2;
                                    OutputJSON.aryUnits[k-1].KB = OutputJSON.aryUnits[k-1].Pitch2Su;         //格子本数
                                }
                                //左はね出し
                                if(j == 1)
                                {
                                    //左端ユニット
                                    //☆☆
                                    if(tIn.Osamari == "独立")
                                    {
                                        OutputJSON.aryUnits[k-1].ZT = OutputJSON.tOut.a + (tIn.Mituke / 2) - OutputJSON.aryUnits[k-1].Kousa;
                                    }
                                    else
                                    {
                                        OutputJSON.aryUnits[k-1].ZT = OutputJSON.tOut.a - tIn.c + (tIn.Mituke / 2) - OutputJSON.aryUnits[k-1].Kousa;
                                    }
                                }
                                else
                                {
                                    OutputJSON.aryUnits[k-1].ZT = OutputJSON.aryUnits[k-1].ZM - (tIn.Mituke / 2) - OutputJSON.aryUnits[k-1].Kousa;
                                }
                                //右はね出し
                                if (j == lngWUnitSu)
                                {
                                    //右端ユニット
                                    //☆☆
                                    if (tIn.Osamari == "独立")
                                    {
                                        OutputJSON.aryUnits[k-1].YT = OutputJSON.tOut.b + (tIn.Mituke / 2);
                                    }
                                    else
                                    {
                                        OutputJSON.aryUnits[k-1].YT = OutputJSON.tOut.b - tIn.d + (tIn.Mituke / 2);
                                    }
                                }
                                else
                                {
                                    OutputJSON.aryUnits[k-1].YT = tIn.Mituke / 2;
                                }
                                //格子重量(g)=(MY×単位重量(格子別)×格子本数)÷1000＋胴縁重量(1000g)
                                if(OutputJSON.aryUnits[k-1].KA < 0)
                                {
                                    if(OutputJSON.aryUnits[k-1].KB < 0){
                                        OutputJSON.aryUnits[k-1].Wait = RoundDown((OutputJSON.aryUnits[k-1].MY * tIn.TWait * 0) / 1000 + DoubutiWait, 0);
                                    }
                                    else
                                    {
                                        OutputJSON.aryUnits[k-1].Wait = RoundDown((OutputJSON.aryUnits[k-1].MY * tIn.TWait * OutputJSON.aryUnits[k-1].KB) / 1000 + DoubutiWait, 0);
                                    }
                                }
                                else
                                {
                                    if (OutputJSON.aryUnits[k - 1].KB < 0)
                                    {
                                        OutputJSON.aryUnits[k - 1].Wait = RoundDown((OutputJSON.aryUnits[k - 1].MY * tIn.TWait * OutputJSON.aryUnits[k - 1].KA) / 1000 + DoubutiWait, 0);
                                    }
                                    else
                                    {
                                        OutputJSON.aryUnits[k - 1].Wait = RoundDown((OutputJSON.aryUnits[k - 1].MY * tIn.TWait * (OutputJSON.aryUnits[k - 1].KA + OutputJSON.aryUnits[k - 1].KB)) / 1000 + DoubutiWait, 0);
                                    }

                                        
                                }
                                if(OutputJSON.aryUnits[k-1].Wait > MaxWait)
                                {
                                    OutputJSON.tOut.ReCalcCnt2 = OutputJSON.tOut.ReCalcCnt2 + 1;
                                    blnMaxOver = true;
                                }
                            }
                        }
                        //重量がMaxオーバーの場合、ユニット数を＋1して再計算
                        if (blnMaxOver)
                        {
                            lngWUnitSu = lngWUnitSu + 1;
                            goto ReSeparateKT;
                        }
                        //全体情報設定
     //                   lngUnit = lngTotalUnitSu;    //合計ユニット数
                        OutputJSON.tOut.UnitSuH = lngHUnitSu;
                        OutputJSON.tOut.UnitSuW = lngWUnitSu;
                        OutputJSON.tOut.ReCalcCnt3 = lngKHSuUnder;
                        OutputJSON.tOut.MaxW = tIn.MaxW;
                        break;
                    case "横格子":
                        //壁内/横格子のユニット分割処理
                        lngKHSuUnder = 0;                       //格子本数下限未満ループ回数
                        //格子W計算
                        //☆☆
                        if(tIn.Osamari == "独立")
                        {
                            lngKousiW = tIn.l + tIn.c + tIn.d;
                        }
                        else
                        {
                            lngKousiW = tIn.l - tIn.a - tIn.b;
                        }
                        //①横方向の分割
ReSeparateWY:
                        lngWUnitSu = (int)RoundUp((double)lngKousiW / (double)tIn.MaxW, 0);
                        lngKLenW = RoundDown((lngKousiW - (tIn.YSukima * (lngWUnitSu - 1))) / lngWUnitSu, 0);
                        lngAmari = lngKousiW - (tIn.YSukima * (lngWUnitSu - 1)) - (lngKLenW * lngWUnitSu);
                        //②横ユニットの作成
                        lngUnitYWk = 0;
                        aryUnitYWk = new List<aryUnit>();
                        for(i = 1;i< lngWUnitSu; i++)
                        {
                            lngUnitYWk = lngUnitYWk + 1;
                            aryUnitYWk[lngUnitYWk].No = lngUnitYWk;
                            if(lngAmari > 0 && (i > lngWUnitSu - lngAmari))
                            {
                                aryUnitYWk[lngUnitYWk].MY = lngKLenW + 1;
                            }
                            else
                            {
                                aryUnitYWk[lngUnitYWk].MY = lngKLenW + 0;
                            }
                            if (i > 1 && i!= lngWUnitSu)
                            {
                                aryUnitYWk[lngUnitYWk].Width = aryUnitYWk[lngUnitYWk].MY + tIn.YSukima;
                            }
                            else
                            {
                                if(lngWUnitSu == 1)
                                {
                                    aryUnitYWk[lngUnitYWk].Width = aryUnitYWk[lngUnitYWk].MY;
                                }
                                else
                                {
                                    aryUnitYWk[lngUnitYWk].Width = aryUnitYWk[lngUnitYWk].MY + (tIn.YSukima / 2);
                                }
                            }
                            //胴縁本数判定
                            aryUnitYWk[lngUnitYWk].DoubutiSu = 0;
                            if(150 <= aryUnitYWk[lngUnitYWk].MY && aryUnitYWk[lngUnitYWk].MY <= 2000){
                                aryUnitYWk[lngUnitYWk].DoubutiSu = 2;
                            }
                            else if (2000 <= aryUnitYWk[lngUnitYWk].MY && aryUnitYWk[lngUnitYWk].MY <= 3000)
                            {
                                aryUnitYWk[lngUnitYWk].DoubutiSu = 3;
                            }
                            else if (3000 <= aryUnitYWk[lngUnitYWk].MY && aryUnitYWk[lngUnitYWk].MY <= 4000)
                            {
                                aryUnitYWk[lngUnitYWk].DoubutiSu = 4;
                            }
                            else if (4000 <= aryUnitYWk[lngUnitYWk].MY && aryUnitYWk[lngUnitYWk].MY <= 4400)
                            {
                                aryUnitYWk[lngUnitYWk].DoubutiSu = 5;
                            }
                            //胴縁ピッチ
                            //はりを決定
                            arytypHariSelect arytypHariSelectWK = HariSelect(tIn);
                            strHari = arytypHariSelectWK.Hari;
                            lngHMituke = arytypHariSelectWK.Mituke;
                            lngHMikomi = arytypHariSelectWK.Mikomi;
                            lngDPitchMin = arytypHariSelectWK.DPitchMin;
                            lngHPitchMin = arytypHariSelectWK.HPitchMin;

                            if (((aryUnitYWk[lngUnitYWk].DoubutiSu + 1) * lngDPitchMin) <= aryUnitYWk[lngUnitYWk].MY && aryUnitYWk[lngUnitYWk].MY < ((aryUnitYWk[lngUnitYWk].DoubutiSu - 1) * 150 + 300) ){
                                //小さいケース①：【（胴縁本数+1)×胴縁最小ピッチ　≦　格子長さMY　＜（胴縁本数-1)×150+300の場合】
                                lngDPitchK = RoundDown(aryUnitYWk[lngUnitYWk].MY / (aryUnitYWk[lngUnitYWk].DoubutiSu + 1), 0);
                                lngDAmari = aryUnitYWk[lngUnitYWk].MY - (lngDPitchK * (aryUnitYWk[lngUnitYWk].DoubutiSu + 1));
                                aryUnitYWk[lngUnitYWk].ZF = -1;    //胴縁ピッチ1：-1は表示しない（値なし）の意味
                                aryUnitYWk[lngUnitYWk].ZG = -1;    //胴縁ピッチ2：-1は表示しない（値なし）の意味
                                aryUnitYWk[lngUnitYWk].ZI = -1;    //胴縁ピッチ3：-1は表示しない（値なし）の意味
                                aryUnitYWk[lngUnitYWk].ZL = -1;    //胴縁ピッチ4：-1は表示しない（値なし）の意味
                                aryUnitYWk[lngUnitYWk].ZS = lngDPitchK;
                                if(lngDAmari > 0)
                                {
                                    aryUnitYWk[lngUnitYWk].YS = lngDPitchK + 1;
                                }
                                else
                                {
                                    aryUnitYWk[lngUnitYWk].YS = lngDPitchK + 0;
                                }
                                if(tIn.Kousi == "横格子ルーバー")
                                {
                                    //胴縁ピッチ4
                                    if (lngDAmari > 1)
                                    {
                                        aryUnitYWk[lngUnitYWk].ZL = lngDPitchK + 1;
                                    }
                                    else
                                    {
                                        aryUnitYWk[lngUnitYWk].ZL = lngDPitchK + 0;
                                    }
                                    
                                }
                                else
                                {
                                    //胴縁ピッチ1
                                    if (lngDAmari > 1)
                                    {
                                        aryUnitYWk[lngUnitYWk].ZF = lngDPitchK + 1;
                                    }
                                    else
                                    {
                                        aryUnitYWk[lngUnitYWk].ZF = lngDPitchK + 0;
                                    }
                                }
                            }else if(aryUnitYWk[lngUnitYWk].MY < ((aryUnitYWk[lngUnitYWk].DoubutiSu + 1) * lngDPitchMin))
                            {
                                //小さいケース②：【格子長さMY　＜（胴縁本数+1)×胴縁最小ピッチの場合】
                                lngDPitchK = lngDPitchMin;
                                aryUnitYWk[lngUnitYWk].ZF = -1;    //胴縁ピッチ1：-1は表示しない（値なし）の意味
                                aryUnitYWk[lngUnitYWk].ZG = -1;    //胴縁ピッチ2：-1は表示しない（値なし）の意味
                                aryUnitYWk[lngUnitYWk].ZI = -1;    //胴縁ピッチ3：-1は表示しない（値なし）の意味
                                aryUnitYWk[lngUnitYWk].ZL = -1;    //胴縁ピッチ4：-1は表示しない（値なし）の意味
                                aryUnitYWk[lngUnitYWk].ZS = RoundDown((aryUnitYWk[lngUnitYWk].MY - lngDPitchK) / 2, 0);
                                aryUnitYWk[lngUnitYWk].YS = (int)RoundUp((double)(aryUnitYWk[lngUnitYWk].MY - lngDPitchK) / 2, 0);
                                if(tIn.Kousi == "横格子ルーバー")
                                {
                                    aryUnitYWk[lngUnitYWk].ZL = lngDPitchK;    //胴縁ピッチ4
                                }
                                else
                                {
                                    aryUnitYWk[lngUnitYWk].ZF = lngDPitchK;    //胴縁ピッチ1
                                }
                            }
                            else
                            {
                                //通常ケース
                                //胴縁両端位置決定（横格子限定）
                                //◎◎
                                if(strHari == "胴縁取付材" && tIn.Osamari == "壁内"){
                                    lngDHasiItiL = DoubutiHasiItiKB - tIn.a;
                                    lngDHasiItiR = DoubutiHasiItiKB - tIn.b;
                                }
                                else
                                {
                                    if(tIn.Osamari == "独立")
                                    {
                                        //最左のみ
                                        if (tIn.LDeiri == "出隅" && tIn.LKatiMake == "勝" && i == 1){
                                            lngDHasiItiL = DoubutiHasiItiDK;
                                        }
                                        else
                                        {
                                            lngDHasiItiL = DoubutiHasiIti;
                                        }
                                        //最右のみ
                                        if(tIn.RDeiri == "出隅" && tIn.RKatiMake == "勝" && i == lngWUnitSu){
                                            lngDHasiItiR = DoubutiHasiItiDK;
                                        }
                                        else
                                        {
                                            lngDHasiItiR = DoubutiHasiIti;
                                        }
                                    }
                                    else
                                    {
                                        //独立以外
                                        lngDHasiItiL = DoubutiHasiIti;
                                        lngDHasiItiR = DoubutiHasiIti;
                                    }
                                }
                                //◎◎
                                lngDPitchK = RoundDown((aryUnitYWk[lngUnitYWk].MY - (lngDHasiItiL + lngDHasiItiR)) / (aryUnitYWk[lngUnitYWk].DoubutiSu - 1), 0);
                                lngDAmari = (aryUnitYWk[lngUnitYWk].MY - (lngDHasiItiL + lngDHasiItiR)) - (lngDPitchK * (aryUnitYWk[lngUnitYWk].DoubutiSu - 1));
                                n = aryUnitYWk[lngUnitYWk].DoubutiSu;
                                aryUnitYWk[lngUnitYWk].ZF = -1;    //胴縁ピッチ1：-1は表示しない（値なし）の意味
                                aryUnitYWk[lngUnitYWk].ZG = -1;    //胴縁ピッチ2：-1は表示しない（値なし）の意味
                                aryUnitYWk[lngUnitYWk].ZI = -1;    //胴縁ピッチ3：-1は表示しない（値なし）の意味
                                aryUnitYWk[lngUnitYWk].ZL = -1;    //胴縁ピッチ4：-1は表示しない（値なし）の意味
                                aryUnitYWk[lngUnitYWk].ZS = lngDHasiItiL;  //◎◎
                                aryUnitYWk[lngUnitYWk].YS = lngDHasiItiR;  //◎◎
                                for(m = 1;m < aryUnitYWk[lngUnitYWk].DoubutiSu - 1;m++)
                                {
                                    n = n - 1;
                                    //余りは上、右(ZL)から配布
                                    if(tIn.Kousi == "横格子ルーバー")
                                    {
                                        //横格子ルーバーの場合、ZLが最左（なので余りの割り振りはZFから）
                                        switch (n)
                                        {
                                            case 1:
                                                //胴縁ピッチ1
                                                if (lngDAmari > 0 && m > (aryUnitYWk[lngUnitYWk].DoubutiSu - 1) - lngDAmari)
                                                {
                                                    aryUnitYWk[lngUnitYWk].ZF = lngDPitchK + 1;
                                                }
                                                else
                                                {
                                                    aryUnitYWk[lngUnitYWk].ZF = lngDPitchK + 0;
                                                }
                                                break;
                                            case 2:
                                                //胴縁ピッチ2
                                                if (lngDAmari > 0 && m > (aryUnitYWk[lngUnitYWk].DoubutiSu - 1) - lngDAmari)
                                                {
                                                    aryUnitYWk[lngUnitYWk].ZG = lngDPitchK + 1;
                                                }
                                                else
                                                {
                                                    aryUnitYWk[lngUnitYWk].ZG = lngDPitchK + 0;
                                                }
                                                break;
                                            case 3:
                                                //胴縁ピッチ3
                                                if (lngDAmari > 0 && m > (aryUnitYWk[lngUnitYWk].DoubutiSu - 1) - lngDAmari)
                                                {
                                                    aryUnitYWk[lngUnitYWk].ZI = lngDPitchK + 1;
                                                }
                                                else
                                                {
                                                    aryUnitYWk[lngUnitYWk].ZI = lngDPitchK + 0;
                                                }
                                                break;
                                            case 4:
                                                //胴縁ピッチ4
                                                if (lngDAmari > 0 && m > (aryUnitYWk[lngUnitYWk].DoubutiSu - 1) - lngDAmari)
                                                {
                                                    aryUnitYWk[lngUnitYWk].ZL = lngDPitchK + 1;
                                                }
                                                else
                                                {
                                                    aryUnitYWk[lngUnitYWk].ZL = lngDPitchK + 0;
                                                }
                                                break;
                                            default:
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        //その他格子の場合、ZFが最左
                                        switch (n)
                                        {
                                            case 1:
                                                //胴縁ピッチ1
                                                if (lngDAmari > 0 && n > (aryUnitYWk[lngUnitYWk].DoubutiSu - 1) - lngDAmari)
                                                {
                                                    aryUnitYWk[lngUnitYWk].ZF = lngDPitchK + 1;
                                                }
                                                else
                                                {
                                                    aryUnitYWk[lngUnitYWk].ZF = lngDPitchK + 0;
                                                }
                                                break;
                                            case 2:
                                                //胴縁ピッチ2
                                                if (lngDAmari > 0 && n > (aryUnitYWk[lngUnitYWk].DoubutiSu - 1) - lngDAmari)
                                                {
                                                    aryUnitYWk[lngUnitYWk].ZG = lngDPitchK + 1;
                                                }
                                                else
                                                {
                                                    aryUnitYWk[lngUnitYWk].ZG = lngDPitchK + 0;
                                                }
                                                break;
                                            case 3:
                                                //胴縁ピッチ3
                                                if (lngDAmari > 0 && n > (aryUnitYWk[lngUnitYWk].DoubutiSu - 1) - lngDAmari)
                                                {
                                                    aryUnitYWk[lngUnitYWk].ZI = lngDPitchK + 1;
                                                }
                                                else
                                                {
                                                    aryUnitYWk[lngUnitYWk].ZI = lngDPitchK + 0;
                                                }
                                                break;
                                            case 4:
                                                //胴縁ピッチ4
                                                if (lngDAmari > 0 && n > (aryUnitYWk[lngUnitYWk].DoubutiSu - 1) - lngDAmari)
                                                {
                                                    aryUnitYWk[lngUnitYWk].ZL = lngDPitchK + 1;
                                                }
                                                else
                                                {
                                                    aryUnitYWk[lngUnitYWk].ZL = lngDPitchK + 0;
                                                }
                                                break;
                                            default:
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                        //③格子割り当て（縦方向）
                        if(tIn.KPitchText == "")
                        {
                            if(tIn.YMode == DekisunYuusen)
                            {
                                //出来寸優先の場合
                                //ピッチバリエーション取得
                                //☆☆
                                if (tIn.Osamari == "独立")
                                {
                                    arytypPitchs = GetPitchCalc(tIn.Kousi, tIn.Mituke, tIn.H - tIn.SBSukima, tIn.Kpitch, aryPitch);
                                }
                                else
                                {
                                    arytypPitchs = GetPitchCalc(tIn.Kousi, tIn.Mituke, tIn.H, tIn.Kpitch, aryPitch);
                                }
                                //候補採用優先順位
                                //1：ピッチ1が画面入力ピッチと同じでピッチ2が使われない（blnDefaultがTrue）
                                //2：ピッチ数の差が最小のもの（blnDefaultがTrue）
                                //3：ピッチ1が画面入力ピッチと同じものが含まれている
                                //4：ピッチ1が画面入力ピッチの直近上位
                                j = 0;
                                for (i = 0; i < arytypPitchs.Count; i++)
                                {
                                    if (arytypPitchs[i].blnDefault)
                                    {
                                        j = i;
                                        break;
                                    }
                                    if (arytypPitchs[i].lngPitch1 >= tIn.Kpitch && j == 0)
                                    {
                                        j = i;
                                    }
                                }
                                //各ピッチ設定
                                OutputJSON.tOut.KPitch1 = aryPitch[j].lngPitch1;
                                OutputJSON.tOut.KPitch2 = aryPitch[j].lngPitch2;
                                //各ピッチ数設定
                                OutputJSON.tOut.KPitch1 = aryPitch[j].lngPitch1;
                                OutputJSON.tOut.KPitch2 = aryPitch[j].lngPitch2;
                                //各ピッチ数設定
                                OutputJSON.tOut.KPitch1Su = aryPitch[j].lngPitch1Su;
                                OutputJSON.tOut.KPitch2Su = aryPitch[j].lngPitch2Su;
                                //ピッチ数計算
                                OutputJSON.tOut.KPitchSu = OutputJSON.tOut.KPitch1Su + OutputJSON.tOut.KPitch2Su;
                                //格子本数計算
                                OutputJSON.tOut.KHonsu = OutputJSON.tOut.KPitchSu + 1;
                                //胴縁Ｌ
                                OutputJSON.tOut.H = tIn.H;
                                //☆☆
                                //下端隙間
                                OutputJSON.tOut.SBSukima = tIn.SBSukima;
                            }
                            else
                            {
                                //ピッチ優先の場合
                                //割付範囲計算
                                //☆☆
                                if (tIn.Osamari == "独立")
                                {
                                    lngWaritukeW = tIn.H - tIn.SBSukima - tIn.Mituke;
                                }
                                else
                                {
                                    lngWaritukeW = tIn.H - tIn.Mituke;
                                }
                                //ピッチ数計算
                                OutputJSON.tOut.KPitchSu = RoundDown(lngWaritukeW / tIn.Kpitch, 0);
                                //格子本数計算
                                OutputJSON.tOut.KHonsu = OutputJSON.tOut.KPitchSu + 1;
                                //各ピッチ数設定
                                OutputJSON.tOut.KPitch1Su = OutputJSON.tOut.KPitchSu;
                                OutputJSON.tOut.KPitch2Su = 0;
                                //各ピッチ設定
                                OutputJSON.tOut.KPitch1 = tIn.Kpitch;
                                OutputJSON.tOut.KPitch2 = 0;
                                //☆☆
                                //胴縁Ｌ、下端隙間
                                if (tIn.Osamari == "独立")
                                {
                                    //独立でピッチ優先の場合は格子Hが入力Hより小さくなることがあるので、隙間で調整する
                                    OutputJSON.tOut.H = tIn.H;
                                    OutputJSON.tOut.SBSukima = OutputJSON.tOut.H - (OutputJSON.tOut.KPitchSu * tIn.Kpitch) + tIn.Mituke;
                                }
                                else
                                {
                                    OutputJSON.tOut.H = (OutputJSON.tOut.KPitchSu * tIn.Kpitch) + tIn.Mituke;
                                    OutputJSON.tOut.SBSukima = tIn.SBSukima;
                                }
                            }
                        }
                        else
                        {
                            //ピッチ選択画面からの戻り（出来寸優先のみ）
                            //☆☆
                            if(tIn.Osamari == "独立")
                            {
                                lngKousiW = tIn.l + tIn.c + tIn.d;
                            }
                            else
                            {
                                lngKousiW = tIn.l - tIn.a - tIn.b;
                            }
                            i = tIn.KPitchText.IndexOf("×");
                            j = tIn.KPitchText.IndexOf("／");
                            if(i > 0)
                            {
                                k = tIn.KPitchText.Substring(i+1, tIn.KPitchText.Length).IndexOf("×");
                            }
                            //各ピッチ設定
                            lngKPitch1 = tIn.KPitchText.Substring(1, i - 1).Length;
                            if (k > 0)
                            {
                                lngKPitch2 = tIn.KPitchText.Substring(j + 1, k - j - 1).Length;
                            }
                            else
                            {
                                lngKPitch2 = -1; //マイナスはピッチ２が使われていないことを示す;
                            }
                            //各ピッチ数設定
                            if (k > 0)
                            {
                                lngKPitch1Su = tIn.KPitchText.Substring(i + 1, j - i - 1).Length;
                                lngKPitch2Su = tIn.KPitchText.Substring(k + 1).Length;
                            }
                            else
                            {
                                lngKPitch1Su = tIn.KPitchText.Substring(i + 1, tIn.KPitchText.Length).Length;
                                lngKPitch2Su = 0;
                            }
                            //ピッチ数計算
                            lngKPitchsu = lngKPitch1Su + lngKPitch2Su;
                            //格子本数計算
                            lngKHonsu = lngKPitchsu + 1;
                            //TOut設定
                            OutputJSON.tOut.H = tIn.H;
                            OutputJSON.tOut.a = tIn.a;
                            OutputJSON.tOut.b = tIn.b;
                            OutputJSON.tOut.c = tIn.c;
                            OutputJSON.tOut.d = tIn.d;
                            OutputJSON.tOut.HHonsu = 0;
                            OutputJSON.tOut.KHonsu = lngKHonsu;
                            OutputJSON.tOut.KPitch1 = lngKPitch1;
                            OutputJSON.tOut.KPitch1Su = lngKPitch1Su;
                            OutputJSON.tOut.KPitch2 = lngKPitch2;
                            OutputJSON.tOut.KPitch2Su = lngKPitch2Su;
                            OutputJSON.tOut.KPitchSu = OutputJSON.tOut.KPitch1Su + OutputJSON.tOut.KPitch2Su;
                            OutputJSON.tOut.Sa = 0;
                            //☆☆
                            if(tIn.Osamari == "独立")
                            {
                                OutputJSON.tOut.W = tIn.l + tIn.c + tIn.d;
                            }
                            else
                            {
                                OutputJSON.tOut.W = tIn.l - tIn.c + tIn.d;
                            }
                            //☆☆
                            //下端隙間
                            OutputJSON.tOut.SBSukima = tIn.SBSukima;
                        }
                        OutputJSON.tOut.ReCalcCnt1 = 0;
                        OutputJSON.tOut.ReCalcCnt2 = 0;
                        OutputJSON.tOut.ReCalcCnt3 = 0;
                        //④縦方向の分割（胴縁分割）
                        //☆☆
                        if(tIn.Osamari == "独立")
                        {
                            //独立はHMax=2800であり、希望Hも指定しないので縦分割されない
                            lngHUnitSu = 1;
                        }
                        else
                        {
                            lngHUnitSu = (int)RoundUp((double)OutputJSON.tOut.H / (double)tIn.MaxH, 0);
                        }
ReSeparateKY:
                        lngKHonsu = RoundDown(OutputJSON.tOut.KHonsu / lngHUnitSu, 0);
                        lngKAmari = OutputJSON.tOut.KHonsu - (lngHUnitSu * lngKHonsu);
                        //初期化
                        lngUnitWk = lngUnitYWk;
                        aryUnitWk = new List<aryUnit>();
                        for (i = 0; i < lngUnitWk;i++)
                        {
                            //ユニット分割横方向ワークを試行用ワークへコピー
                            aryUnitWk[i].No = aryUnitYWk[i].No;
                            aryUnitWk[i].Pitch1 = aryUnitYWk[i].Pitch1;
                            aryUnitWk[i].Pitch1Su = aryUnitYWk[i].Pitch1Su;
                            aryUnitWk[i].Pitch2 = aryUnitYWk[i].Pitch2;
                            aryUnitWk[i].Pitch2Su = aryUnitYWk[i].Pitch2Su;
                            aryUnitWk[i].Honsu = aryUnitYWk[i].Honsu;
                            aryUnitWk[i].Width = aryUnitYWk[i].Width;
                            aryUnitWk[i].Kousa = aryUnitYWk[i].Kousa;
                            aryUnitWk[i].Sx = aryUnitYWk[i].Sx;
                            aryUnitWk[i].Ex = aryUnitYWk[i].Ex;
                            aryUnitWk[i].Height = aryUnitYWk[i].Height;
                            aryUnitWk[i].Sy = aryUnitYWk[i].Sy;
                            aryUnitWk[i].Ey = aryUnitYWk[i].Ey;
                            aryUnitWk[i].DoubutiSu = aryUnitYWk[i].DoubutiSu;
                            aryUnitWk[i].MX = aryUnitYWk[i].MX;
                            aryUnitWk[i].ZT = aryUnitYWk[i].ZT;
                            aryUnitWk[i].ZM = aryUnitYWk[i].ZM;
                            aryUnitWk[i].KA = aryUnitYWk[i].KA;
                            aryUnitWk[i].ZN = aryUnitYWk[i].ZN;
                            aryUnitWk[i].KB = aryUnitYWk[i].KB;
                            aryUnitWk[i].YT = aryUnitYWk[i].YT;
                            aryUnitWk[i].MY = aryUnitYWk[i].MY;
                            aryUnitWk[i].ZS = aryUnitYWk[i].ZS;
                            aryUnitWk[i].ZF = aryUnitYWk[i].ZF;
                            aryUnitWk[i].ZG = aryUnitYWk[i].ZG;
                            aryUnitWk[i].ZI = aryUnitYWk[i].ZI;
                            aryUnitWk[i].ZL = aryUnitYWk[i].ZL;
                            aryUnitWk[i].YS = aryUnitYWk[i].YS;
                            aryUnitWk[i].Wait = aryUnitYWk[i].Wait;
                        }
                        lngHeight = 0;
                        blnMaxOver = false;           //ユニットMAXオーバーチェック用
                        blnKHSuUnder = false;         //格子本数下限未満チェック用
                        //胴縁公差
                        lngKousa = GetDoubutiKousa(tIn, OutputJSON, OutputJSON.tOut.H, lngHUnitSu);
                        //⑤縦ユニットの作成
                        lngKPitch1ZSu = OutputJSON.tOut.KPitch1Su;
                        lngKPitch2ZSu = OutputJSON.tOut.KPitch2Su;
                        k = 0;                       //aryUnitのインデクス
                        for(i = 1;i< lngHUnitSu;i++)
                        {
                            //この段の格子本数（余りを上から1本ずつ割り当て）
                            if(lngKAmari > 0 && i <= lngKAmari){
                                lngKHonsuHD = lngKHonsu + 1;
                            }
                            else
                            {
                                lngKHonsuHD = lngKHonsu + 0;
                            }
                            if(i == 1)
                            {
                                lngKPitchSuWk = lngKHonsuHD - 1; //ピッチ数は本数－1なので
                            }
                            else
                            {
                                lngKPitchSuWk = lngKHonsuHD; //ピッチ数は本数と同じなので
                            }
                            //ピッチ数計算
                            lngKPitch1 = -1;     //-1は表示しないことを示す
                            lngKPitch1Su = 0;
                            lngKPitch2 = -1;     //-1は表示しないことを示す
                            lngKPitch2Su = 0;
                            if(lngKPitch1ZSu >= lngKHonsuHD - 1)
                            {
                                //全てピッチ1でまかなう
                                lngKPitch1 = OutputJSON.tOut.KPitch1;
                                if(i == 1){
                                    lngKPitch1Su = lngKHonsuHD - 1;
                                }
                                else
                                {
                                    lngKPitch1Su = lngKHonsuHD;
                                }
                                lngKPitch1ZSu = lngKPitch1ZSu - lngKPitch1Su;
                            }
                            else
                            {
                                //ピッチ2も使う
                                if (lngKPitch1ZSu > 0)
                                {
                                    //ピッチ1の残あり
                                    lngKPitch1 = OutputJSON.tOut.KPitch1;
                                    lngKPitch1Su = lngKPitch1ZSu;
                                    lngKPitch1ZSu = 0;   //ピッチ1を使い切った
                                    lngKPitchSuWk = lngKPitchSuWk - lngKPitch1Su;
                                }
                                lngKPitch2 = OutputJSON.tOut.KPitch2;
                                if(lngKPitch2ZSu > 0)
                                {
                                    //ピッチ2残あり
                                    if(lngKPitch2ZSu >= lngKPitchSuWk)
                                    {
                                        //不足分をピッチ2残で全てまかなえる
                                        lngKPitch2Su = lngKPitchSuWk;
                                        lngKPitch2ZSu = lngKPitch2ZSu - lngKPitchSuWk;
                                    }
                                    else
                                    {
                                        //ピッチ2残が足りない※この状況には基本的にならないはず
                                        lngKPitch2Su = lngKPitch2ZSu;
                                        lngKPitch2ZSu = 0;   //ピッチ2を使い切った
                                    }
                                }
                            }
                            //横へ展開
                            for(j = 1;j< lngWUnitSu;j++)
                            {
                                if(i == 1)
                                {
                                    k = j;
                                }
                                else
                                {
                                    lngUnitWk = lngUnitWk + 1;
                                    k = lngUnitWk;
                                    aryUnitWk[lngUnitWk].No = lngUnitWk;
                                }
                                if(j != k)
                                {
                                    //横情報は1段目のコピー
                                    aryUnitWk[lngUnitWk].MY = aryUnitWk[j].MY;               //格子長
                                    aryUnitWk[lngUnitWk].Width = aryUnitWk[j].Width;         //ユニットＷ※隙間含む
                                    aryUnitWk[lngUnitWk].DoubutiSu = aryUnitWk[j].DoubutiSu; //胴縁本数
                                    aryUnitWk[lngUnitWk].ZF = aryUnitWk[j].ZF;               //胴縁ピッチ1（左）
                                    aryUnitWk[lngUnitWk].ZG = aryUnitWk[j].ZG;               //胴縁ピッチ2（↓）
                                    aryUnitWk[lngUnitWk].ZI = aryUnitWk[j].ZI;               //胴縁ピッチ3（↓）
                                    aryUnitWk[lngUnitWk].ZL = aryUnitWk[j].ZL;               //胴縁ピッチ4（右）
                                    aryUnitWk[lngUnitWk].ZS = aryUnitWk[j].ZS;               //格子はね出し（左）
                                    aryUnitWk[lngUnitWk].YS = aryUnitWk[j].YS;               //格子はね出し（右）
                                }
                                //縦情報設定
                                aryUnitWk[lngUnitWk].Pitch1 = lngKPitch1;                    //ピッチ1
                                aryUnitWk[lngUnitWk].Pitch1Su = lngKPitch1Su;                //ピッチ1数
                                aryUnitWk[lngUnitWk].Pitch2 = lngKPitch2;                    //ピッチ2
                                aryUnitWk[lngUnitWk].Pitch2Su = lngKPitch2Su;                //ピッチ2数
                                if (j == 1)
                                {
                                    aryUnitWk[lngUnitWk].Honsu = lngKHonsuHD + 1;            //格子本数
                                }
                                else
                                {
                                    aryUnitWk[lngUnitWk].Honsu = lngKHonsuHD;                //格子本数
                                }
                                aryUnitWk[lngUnitWk].Kousa = 0;                              //公差
                                if(tIn.Kousi == "クリア格子")
                                {
                                    if(lngHUnitSu == 2)
                                    {
                                        //クリア格子で2分割の場合は下側胴縁のみマイナス
                                        if(i == lngHUnitSu)
                                        {
                                            aryUnitWk[lngUnitWk].Kousa = lngKousa;           //公差
                                        }
                                    }
                                    else
                                    {
                                        //クリア格子でも2分割でない場合は通常通り
                                        if(lngKousa > 0 && (i > lngHUnitSu - lngKousa))
                                        {
                                            aryUnitWk[lngUnitWk].Kousa = 1;                  //公差
                                        }
                                    }
                                }
                                else
                                {
                                    if(lngKousa > 0 && (i > lngHUnitSu - lngKousa)){
                                        aryUnitWk[lngUnitWk].Kousa = 1;                  //公差
                                    }
                                }
                                if (i == 1)
                                {
                                    //一番上のユニットの胴縁長
                                    lngDLen = ((aryUnitWk[lngUnitWk].Pitch1 * aryUnitWk[lngUnitWk].Pitch1Su) + (aryUnitWk[lngUnitWk].Pitch2 * aryUnitWk[lngUnitWk].Pitch2Su)) + tIn.Mituke;
                                    if (tIn.Kousi == "横格子面材Ａ")
                                    {
                                        //横格子面材は見付が奇数のため、胴縁を1mm増やす※一番上のユニットは格子より胴縁が1mm飛び出す
                                        lngDLen = lngDLen + 1;
                                    }
                                }
                                else
                                {
                                    //その他のユニットの胴縁長※上伸ばし
                                    lngDLen = ((aryUnitWk[lngUnitWk].Pitch1 * aryUnitWk[lngUnitWk].Pitch1Su) + (aryUnitWk[lngUnitWk].Pitch2 * aryUnitWk[lngUnitWk].Pitch2Su));
                                }
                                aryUnitWk[lngUnitWk].MX = lngDLen - aryUnitWk[lngUnitWk].Kousa;
                                aryUnitWk[lngUnitWk].Height = lngDLen;                       //横格子の縦接続部分は隙間無し
                                //トータルＨ
                                if(j == 1)
                                {
                                    lngHeight = lngHeight + aryUnitWk[lngUnitWk].Height;
                                }
                                if(aryUnitWk[lngUnitWk].MX > tIn.MaxH)
                                {
                                    //高さがMaxオーバーの場合
                                    OutputJSON.tOut.ReCalcCnt1 = OutputJSON.tOut.ReCalcCnt1 + 1;
                                    blnMaxOver = true;
                                }
                                if(i == 1)
                                {
                                    //一番上のユニット
                                    if(tIn.Kousi == "横格子ルーバー")
                                    {
                                        aryUnitWk[lngUnitWk].ZT = 27 - aryUnitWk[lngUnitWk].Kousa;                               //左はね出し(縦)/上はね出し(横)

                                    }else if (tIn.Kousi == "横格子面材Ａ")
                                    {
                                        aryUnitWk[lngUnitWk].ZT = 58 - aryUnitWk[lngUnitWk].Kousa;                               //左はね出し(縦)/上はね出し(横)
                                    }
                                    else
                                    {
                                        aryUnitWk[lngUnitWk].ZT = (tIn.Mituke / 2) - aryUnitWk[lngUnitWk].Kousa;                 //左はね出し(縦)/上はね出し(横)
                                    }
                                }
                                else
                                {
                                    //その他のユニット※上伸ばし
                                    if(aryUnitWk[lngUnitWk].Pitch1 > 0)
                                    {
                                        if (tIn.Kousi == "横格子ルーバー")
                                        {
                                            aryUnitWk[lngUnitWk].ZT = aryUnitWk[lngUnitWk].Pitch1 - 28 - aryUnitWk[lngUnitWk].Kousa;                               //左はね出し(縦)/上はね出し(横)

                                        }
                                        else if (tIn.Kousi == "横格子面材Ａ")
                                        {
                                            aryUnitWk[lngUnitWk].ZT = aryUnitWk[lngUnitWk].Pitch1 - 58 - aryUnitWk[lngUnitWk].Kousa;                               //左はね出し(縦)/上はね出し(横)
                                        }
                                        else
                                        {
                                            aryUnitWk[lngUnitWk].ZT = aryUnitWk[lngUnitWk].Pitch1 - (tIn.Mituke / 2) - aryUnitWk[lngUnitWk].Kousa;                 //左はね出し(縦)/上はね出し(横)
                                        }
                                    }
                                    else
                                    {
                                        if (tIn.Kousi == "横格子ルーバー")
                                        {
                                            aryUnitWk[lngUnitWk].ZT = aryUnitWk[lngUnitWk].Pitch2 - 28 - aryUnitWk[lngUnitWk].Kousa;                               //左はね出し(縦)/上はね出し(横)

                                        }
                                        else if (tIn.Kousi == "横格子面材Ａ")
                                        {
                                            aryUnitWk[lngUnitWk].ZT = aryUnitWk[lngUnitWk].Pitch2 - 58 - aryUnitWk[lngUnitWk].Kousa;                               //左はね出し(縦)/上はね出し(横)
                                        }
                                        else
                                        {
                                            aryUnitWk[lngUnitWk].ZT = aryUnitWk[lngUnitWk].Pitch2 - (tIn.Mituke / 2) - aryUnitWk[lngUnitWk].Kousa;                 //左はね出し(縦)/上はね出し(横)
                                        }
                                    }
                                }
                                if (aryUnitWk[lngUnitWk].Pitch1 > 0)
                                {
                                    aryUnitWk[lngUnitWk].ZM = aryUnitWk[lngUnitWk].Pitch1;                       //格子ピッチ1
                                    //格子本数1
                                    if (i == 1)
                                    {
                                        aryUnitWk[lngUnitWk].KA = aryUnitWk[lngUnitWk].Pitch1Su + 1;
                                    }
                                    else
                                    {
                                        aryUnitWk[lngUnitWk].KA = aryUnitWk[lngUnitWk].Pitch1Su;
                                    }
                                    aryUnitWk[lngUnitWk].ZN = aryUnitWk[lngUnitWk].Pitch2;                       //格子ピッチ2
                                    aryUnitWk[lngUnitWk].KB = aryUnitWk[lngUnitWk].Pitch2Su;                     //格子本数2
                                }
                                else
                                {
                                    aryUnitWk[lngUnitWk].ZM = aryUnitWk[lngUnitWk].Pitch2;                       //格子ピッチ1
                                    aryUnitWk[lngUnitWk].KA = aryUnitWk[lngUnitWk].Pitch2Su;                     //格子本数1
                                    aryUnitWk[lngUnitWk].ZN = -1;                                               //格子ピッチ2
                                    aryUnitWk[lngUnitWk].KB = 0;                                                //格子本数2
                                }
                                if (tIn.Kousi == "横格子ルーバー")
                                {
                                    aryUnitWk[lngUnitWk].YT = 28;                            //左はね出し(縦)/上はね出し(横)
                                }else if (tIn.Kousi == "横格子面材Ａ")
                                {
                                    aryUnitWk[lngUnitWk].YT = 58;                            //左はね出し(縦)/上はね出し(横)
                                }
                                else
                                {
                                    aryUnitWk[lngUnitWk].YT = tIn.Mituke / 2;                //右はね出し(縦)/下はね出し(横)
                                }
                                //格子重量(g)=(MY×単位重量(格子別)×格子本数)÷1000＋胴縁重量(1000g)
                                //重量(g)
                                if (aryUnitWk[lngUnitWk].KA < 0)
                                {
                                    if (aryUnitWk[lngUnitWk].KB < 0)
                                    {
                                        aryUnitWk[lngUnitWk].Wait = RoundDown((aryUnitWk[lngUnitWk].MY * tIn.TWait * 0) / 1000 + DoubutiWait, 0);
                                    }
                                    else
                                    {
                                        aryUnitWk[lngUnitWk].Wait = RoundDown((aryUnitWk[lngUnitWk].MY * tIn.TWait * aryUnitWk[lngUnitWk].KA) / 1000 + DoubutiWait, 0);
                                    }
                                }
                                else
                                {
                                    aryUnitWk[lngUnitWk].Wait = RoundDown((aryUnitWk[lngUnitWk].MY * tIn.TWait * (aryUnitWk[lngUnitWk].KA + aryUnitWk[lngUnitWk].KB)) / 1000 + DoubutiWait, 0);
                                }
                                if(aryUnitWk[lngUnitWk].Wait > MaxWait)
                                {
                                    //重量がMaxオーバーの場合
                                    OutputJSON.tOut.ReCalcCnt2 = OutputJSON.tOut.ReCalcCnt2 + 1;
                                    blnMaxOver = true;
                                }
                                //このユニットの格子本数が3本以上あるか？
                                if(aryUnitWk[lngUnitWk].KA < 0){
                                    if (aryUnitWk[lngUnitWk].KB < 0){
                                        m = 0;
                                    }
                                    else
                                    {
                                        m = aryUnitWk[lngUnitWk].KB;
                                    }
                                }else
                                {
                                    m = aryUnitWk[lngUnitWk].KA + aryUnitWk[lngUnitWk].KB;
                                }
                                if(m < 3)
                                {
                                    if(lngHUnitSu == 1)
                                    {
                                        //縦が１ユニットしかないのに格子本数が足りない異常ケース。このままだとループするので強制終了
                                        OutputJSON.Result.massage = "Ｈ寸法が小さすぎて計算できません。強制終了します。";
                                        OutputJSON.Result.status = "NG";
                                    }
                                    //このケースは横格子面材で重量オーバーを繰り返した場合に訪れる
                                    if(blnKHSuUnder == false)
                                    {
                                        lngKHSuUnder = lngKHSuUnder + 1; //格子本数下限未満ループ回数
                                        //MY＝（格子重量(g)－胴縁重量(g)）÷（単位重量(格子別)(g)×格子本数）
                                        //20Kgの限界MYを計算
                                        lngReSize = RoundDown((20000 - DoubutiWait) / (tIn.TWait * OutputJSON.tOut.KHonsu) * 1000, 0);
                                        blnKHSuUnder = true;
                                    }
                                }
                            }
                        }
                        //DoEvents    '★★★★デバッグ（無限ループ救済）
                        if(blnMaxOver && (blnKHSuUnder==false)){
                            //上限超えの場合、縦のユニット数を＋1して再度割り付けし直し
                            lngHUnitSu = lngHUnitSu + 1;
                            goto ReSeparateKY;
                        }
                        if (blnKHSuUnder)
                        {
                            //格子本数下限未満の場合、WMaxを小さく、縦分割が発生しないようにして再度割り付けし直し
                            tIn.MaxW = lngReSize;
                            //縦分割されないようにする。横格子面材は見付が奇数のため、胴縁が1mm増えるのでその分をMaxHに追加
                            if(OutputJSON.tOut.H > tIn.MaxH)
                            {
                                if(tIn.Kousi == "横格子面材Ａ"){
                                    tIn.MaxH = OutputJSON.tOut.H + 1;
                                }
                                else
                                {
                                    tIn.MaxH = OutputJSON.tOut.H;
                                }
                            }
                            goto ReSeparateWY;
                        }
                        //全体項目の計算
                        //☆☆
                        if (tIn.Osamari == "独立")
                        {
                            OutputJSON.tOut.W = tIn.l + tIn.c + tIn.d;
                            OutputJSON.tOut.SBSukima = OutputJSON.tOut.H - lngHeight;
                        }
                        else
                        {
                            OutputJSON.tOut.W = tIn.l - tIn.a - tIn.b;
                            OutputJSON.tOut.H = lngHeight;
                        }
                        OutputJSON.tOut.a = tIn.a;
                        OutputJSON.tOut.b = tIn.b;
                        OutputJSON.tOut.c = tIn.c;
                        OutputJSON.tOut.d = tIn.d;
                        OutputJSON.tOut.UnitSuW = lngWUnitSu;
                        OutputJSON.tOut.UnitSuH = lngHUnitSu;
                        OutputJSON.tOut.ReCalcCnt3 = lngKHSuUnder;
                        OutputJSON.tOut.MaxW = tIn.MaxW;
                        //ユニット情報設定
                        for (i = 0; i < lngUnitWk; i++)
                        {
                            //ユニット分割横方向ワークを試行用ワークへコピー
                            OutputJSON.aryUnits.Add(aryUnitWk[i]);
                        }
                        break;
                    default:
                        break;
                    }
            }
            
            else if (tIn.Osamari == "独立")
            {

            }
            
            return OutputJSON;
        }

        //計算でピッチを取り出す
        //前提 
        //・ピッチは２種類までとする｡ピッチ１のみ､あるいは､ピッチ１とピッチ２｡
        //・ピッチ１とピッチ２は１mm差
        private List<arytypPitch> GetPitchCalc(string strKousi, int lngKousiMituke, int lngDekihaba, int lngInputPitch, List<arytypPitch> aryPitch)
        {
            int lngPitchHaba = 0;                  //ピッチ幅
            int lngPitchMin = 0;                   //ピッチ可能範囲最小値
            int lngPitchMax = 0;                   //ピッチ可能範囲最大値
            int lngPitchSu = 0;                    //ピッチ数
            int lngPitchSuCalc = 0;                //計算用ピッチ数
            int lngPitchGosa = 0;                  //ピッチ誤差
            int lngPitch2Kasan = 0;                //ピッチ２加算値
            int lngPitch1 = 0;                     //ピッチ１
            int lngPitch1Su = 0;                   //ピッチ１数
            int lngPitch2 = 0;                     //ピッチ２
            int lngPitch2Su = 0;                   //ピッチ２数
            int lngPitch1Sa = 0;                   //入力ピッチとピッチ1のピッチ差
            int lngPitch2Sa = 0;                   //入力ピッチとピッチ2のピッチ差
            int lngWK = 0;
            int lngSave = 0;                       //1：ピッチ１だけで希望値、2：希望値が含まれるがピッチ２も使用、3：その他だけど候補
            Boolean blnFound;
            int lngSaMin = 0;
            int lngSuMax = 0;                     //同一ピッチでのピッチ数の大きい方
            int lngPitchSave = 0;                 //デフォルト合致したピッチ
            Boolean blnPitchEQ;                //デフォルト設定したピッチは入力ピッチと同じ場合True
            int lngDefaultPos = 0;

            //可能ピッチ取得
            switch (strKousi)
            {
                case "２０×３０格子":
                    lngPitchMin = int.Parse(Left(KanouPitch20x30, KanouPitch20x30.IndexOf(",")));
                    lngPitchMax = int.Parse(Mid(KanouPitch20x30, KanouPitch20x30.IndexOf(",")+1));
                    break;
                case "３０×５０格子":
                    lngPitchMin = int.Parse(Left(KanouPitch30x50, KanouPitch30x50.IndexOf(",")));
                    lngPitchMax = int.Parse(Mid(KanouPitch30x50, KanouPitch30x50.IndexOf(",") + 1));
                    break;
                case "５０×５０格子":
                    lngPitchMin = int.Parse(Left(KanouPitch50x50, KanouPitch50x50.IndexOf(",")));
                    lngPitchMax = int.Parse(Mid(KanouPitch50x50, KanouPitch50x50.IndexOf(",") + 1));
                    break;
                case "クリア格子":
                    lngPitchMin = int.Parse(Left(KanouPitchClear, KanouPitchClear.IndexOf(",")));
                    lngPitchMax = int.Parse(Mid(KanouPitchClear, KanouPitchClear.IndexOf(",") + 1));
                    break;
                case "エコリルウッド格子":
                    lngPitchMin = int.Parse(Left(KanouPitchEcoRl, KanouPitchEcoRl.IndexOf(",")));
                    lngPitchMax = int.Parse(Mid(KanouPitchEcoRl, KanouPitchEcoRl.IndexOf(",") + 1));
                    break;
                case "横格子ルーバー":
                    lngPitchMin = int.Parse(Left(KanouPitchLouvr, KanouPitchLouvr.IndexOf(",")));
                    lngPitchMax = int.Parse(Mid(KanouPitchLouvr, KanouPitchLouvr.IndexOf(",") + 1));
                    break;
                case "横格子面材Ａ":
                    lngPitchMin = int.Parse(Left(KanouPitchYokoA, KanouPitchYokoA.IndexOf(",")));
                    lngPitchMax = int.Parse(Mid(KanouPitchYokoA, KanouPitchYokoA.IndexOf(",") + 1));
                    break;
                default:
                    //計算不可なのでExit
                    return null;
            }
            //横格子ルーバー、横格子面材は固定ピッチなので、結果設定して終了
            if (strKousi == "横格子ルーバー" || strKousi == "横格子面材Ａ"){
                arytypPitch aryPitchWk = new arytypPitch();
                aryPitchWk.lngPitch1 = lngPitchMin;
                //ピッチ幅計算
                lngPitchHaba = lngDekihaba - lngKousiMituke;
                //ピッチ数計算
                aryPitchWk.lngPitch1Su = lngPitchHaba / aryPitchWk.lngPitch1;
                aryPitchWk.lngPitch2 = 0;
                aryPitchWk.lngPitch2Su = 0;
                aryPitchWk.blnDefault = true;
                aryPitchWk.lngDefaultSa = 0;
                aryPitch.Add(aryPitchWk);
                return aryPitch;
            }
            //可変ピッチの格子は以下計算
            //ピッチ幅計算
            lngPitchHaba = lngDekihaba - lngKousiMituke;
            //計算ループ
            lngSaMin = 9999999;
            lngSuMax = 0;
            lngDefaultPos = 0;
            blnPitchEQ = false;
            for(int i= lngPitchMin; i<= lngPitchMax; i++)
            {
                //初期化
                lngPitch1 = 0;
                lngPitch1Su = 0;
                lngPitch2 = 0;
                lngPitch2Su = 0;
                lngSave = 0;
                //ピッチ１
                lngPitch1 = i;
                //ピッチ数計算。ピッチに応じて丸めを変更
                if (i == lngPitchMin)
                {
                    lngPitchSu = RoundDown((double)(lngPitchHaba / i), 0);

                }
                else if (i == lngPitchMax)
                {
                    lngPitchSu = (int)RoundUp((double)lngPitchHaba / (double)i, 0);
                }
                else
                {
                    lngPitchSu = Round(lngPitchHaba / i, 0);
                }
                //ピッチ誤差計算
                lngPitchGosa = lngPitchHaba - (i * lngPitchSu);
                //ピッチ１数 ※ピッチ２を計算しないとき
                if(lngPitchGosa == 0)
                {
                    lngPitch1Su = lngPitchSu;
                    if(lngPitch1 == lngInputPitch)
                    {
                        lngSave = 1;
                    }
                    else
                    {
                        lngSave = 3;
                    }
                }
                //計算用ピッチ数計算
                if(i < lngPitchGosa)
                {
                    lngPitchSuCalc = lngPitchSu + 1;
                }
                else
                {
                    lngPitchSuCalc = lngPitchSu;
                }
                //ピッチ２計算
                //誤差があり、1mmプラマイ範囲内に収まる
                if(lngPitchGosa != 0 && lngPitchSuCalc > Math.Abs(lngPitchHaba - (i * lngPitchSuCalc))){
                    //ピッチ２加算値計算
                    lngPitch2Kasan = lngPitchHaba - (i * lngPitchSuCalc);
                    //ピッチ２計算
                    lngPitch2 = i + (lngPitch2Kasan / Math.Abs(lngPitch2Kasan));
                    //ピッチ１数計算
                    lngPitch1Su = lngPitchSu - Math.Abs(lngPitch2Kasan);
                    //ピッチ２数計算
                    lngPitch2Su = lngPitchSu - lngPitch1Su;
                    if(lngPitch2 == lngInputPitch)
                    {
                        lngSave = 2;
                    }
                    else
                    {
                        lngSave = 3;
                    }
                    //最終チェック
                    if((lngPitch1 * lngPitch1Su) + (lngPitch2 * lngPitch2Su) != lngPitchHaba)
                    {
                        lngSave = 0;
                    }
                }
                if(lngSave > 0)
                {
                    //候補の場合
                    //ピッチ１＜ピッチ２に入れ替える
                    if(lngPitch2 != 0 && lngPitch1 > lngPitch2){
                        lngWK = lngPitch1;
                        lngPitch1 = lngPitch2;
                        lngPitch2 = lngWK;
                        lngWK = lngPitch1Su;
                        lngPitch1Su = lngPitch2Su;
                        lngPitch2Su = lngWK;
                    }
                    //結果配列に格納
                    blnFound = false;
                    //既に同じモノがあるか確認
                    for (int j = 0; j < aryPitch.Count; j++)
                    {
                        if(aryPitch[j].lngPitch1 == lngPitch1 && aryPitch[j].lngPitch1Su == lngPitch1Su &&
                           aryPitch[j].lngPitch2 == lngPitch2 && aryPitch[j].lngPitch2Su == lngPitch2Su )
                        {
                            blnFound = true;
                            break;
                        } 
                    }
                    if(blnFound == false)
                    {
                        //同じモノがなかったら格納
                        arytypPitch aryPitchWk = new arytypPitch();
                        aryPitchWk.lngPitch1 = lngPitch1;
                        aryPitchWk.lngPitch1Su = lngPitch1Su;
                        aryPitchWk.lngPitch2 = lngPitch2;
                        aryPitchWk.lngPitch2Su = lngPitch2Su;
                        if(lngPitchSave != lngPitch1 && lngPitchSave!= lngPitch2 && blnPitchEQ){
                            lngSuMax = 0;
                        }
                        //最適ピッチ検出
                        lngPitch1Sa = Math.Abs(lngInputPitch - lngPitch1);
                        lngPitch2Sa = Math.Abs(lngInputPitch - lngPitch2);
                        if(lngPitch1Sa < lngPitch2Sa)
                        {
                            //ピッチ1の方が差が小さい
                            if(lngSaMin >= lngPitch1Sa)
                            {
                                lngSaMin = lngPitch1Sa;
                                if(lngSuMax < lngPitch1Su)
                                {
                                    //かつてあったものよりも優先
                                    lngSuMax = lngPitch1Su;
                                    lngDefaultPos = aryPitch.Count;
                                    lngPitchSave = lngPitch1;
                                    if(lngPitchSave == lngInputPitch)
                                    {
                                        blnPitchEQ = true;
                                    }
                                    else
                                    {
                                        blnPitchEQ = true;
                                    }
                                }
                            }
                        }else if(lngPitch1Sa > lngPitch2Sa)
                        {
                            //ピッチ2の方が差が小さい
                            if(lngSaMin >= lngPitch2Sa)
                            {
                                lngSaMin = lngPitch2Sa;
                                if(lngSuMax < lngPitch2Su)
                                {
                                    //かつてあったものよりも優先
                                    lngSuMax = lngPitch2Su;
                                    lngDefaultPos = aryPitch.Count;
                                    lngPitchSave = lngPitch2;
                                    if(lngPitchSave == lngInputPitch)
                                    {
                                        blnPitchEQ = true;
                                    }
                                    else
                                    {
                                        blnPitchEQ = true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            //差は一緒
                            if(lngPitch1Su >= lngPitch2Su)
                            {
                                //ピッチ1の方がピッチ数が多いか等しい
                                if(lngSaMin >= lngPitch1Sa)
                                {
                                    lngSaMin = lngPitch1Sa;
                                    if(lngSuMax < lngPitch1Su)
                                    {
                                        //かつてあったものよりも優先
                                        lngSuMax = lngPitch1Su;
                                        lngDefaultPos = aryPitch.Count;
                                        lngPitchSave = lngPitch1;
                                       if(lngPitchSave == lngInputPitch)
                                        {
                                            blnPitchEQ = true;
                                        }
                                        else
                                        {
                                            blnPitchEQ = true;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                //ピッチ2の方がピッチ数が多い
                                if(lngSaMin >= lngPitch2Sa)
                                {
                                    lngSaMin = lngPitch2Sa;
                                    if(lngSuMax < lngPitch2Su)
                                    {
                                        //かつてあったものよりも優先
                                        lngSuMax = lngPitch2Su;
                                        lngDefaultPos = aryPitch.Count;
                                        lngPitchSave = lngPitch2;
                                        if (lngPitchSave == lngInputPitch)
                                        {
                                            blnPitchEQ = true;
                                        }
                                        else
                                        {
                                            blnPitchEQ = true;
                                        }
                                    }
                                }
                            }
                        }
                        aryPitch.Add(aryPitchWk);
                    }
                }
            }

            //blnDefaultの選択優先順位
            //入力ピッチとピッチ1orピッチ2の差が小さいもので、
            //差の小さいもののピッチ数が多い方
            //同値なら先に見つかった方
            //1：ピッチ1が画面入力ピッチと同じでピッチ2が使われない
            //2：画面入力ピッチと同じピッチの数が多いモノ（ピッチ数の差が最小のもの）
            if(lngDefaultPos != 0)
            {
                aryPitch[lngDefaultPos].blnDefault = true;
            }

            return aryPitch;
        }
    
        //四捨五入
        private int Round(Double Value, int pos)
        {
            return (int)Math.Round(Value);
        }
        //切り捨て
        private int RoundDown(Double Value, int pos)
        {
            return (int)Math.Floor(Value);
        }
        //切り上げ
        private static double RoundUp(Double Value, int pos)
        {
            double dCoef = System.Math.Pow(10, pos);

            return Value > 0 ? System.Math.Ceiling(Value * dCoef) / dCoef :
                                System.Math.Floor(Value * dCoef) / dCoef;
        }

        /// <summary>
        /// 文字列の指定した位置から末尾までを取得する
        /// </summary>
        /// <param name="str">文字列</param>
        /// <param name="start">開始位置</param>
        /// <returns>取得した文字列</returns>
        private static string Mid(string str, int start)
        {
            if (start <= str.Length)
            {
                return str.Substring(start);
            }

            return string.Empty;
        }

        /// <summary>
        /// 文字列の先頭から指定した長さの文字列を取得する
        /// </summary>
        /// <param name="str">文字列</param>
        /// <param name="len">長さ</param>
        /// <returns>取得した文字列</returns>
        private static string Left(string str, int len)
        {
            if (len < 0)
            {
                throw new ArgumentException("引数'len'は0以上でなければなりません。");
            }
            if (str == null)
            {
                return "";
            }
            if (str.Length <= len)
            {
                return str;
            }
            return str.Substring(0, len);
        }

        //胴縁公差取得
        private int GetDoubutiKousa(InputJSON tIn, OutputJSON OutputJSON, int lngSize, int lngUnitSu)
        {
            double ToritukezaiKousa;
            double DoubutiKousa;
            int lngL;
            int HHonsu=0;
            int HPitch;
            int HAmari;
            int lngHariKanMin;
            int lngToritukezaiL;
            int lngDTZai;
            int lngHari;
            List<aryHari> aryHari = new List<aryHari>();
            int lngHPitchW=0;
            int i;
            int k;
            if(lngUnitSu == 1)
            {
                return 0;
            }
            else
            {
                if(tIn.TateYoko == "縦格子" || tIn.HMode == "はり間連結"){
                    //通常時はHを4400で割れば胴縁が何分割されるかわかる
                    ToritukezaiKousa = RoundUp((double)lngSize / (double)DTZaiL, 0) * Kousa;
                    DoubutiKousa = lngUnitSu * Kousa;
                    return (int)RoundUp((double)ToritukezaiKousa + (double)DoubutiKousa, 0);
                }
                else
                {
                    //はり前連結の場合は胴縁取付材が何分割されるか調べる
                    //梁長
                    lngL = tIn.l - tIn.c - tIn.d;
                    //はり本数を識別（Ｈによるデフォルト値)
                    if(OutputJSON.tOut.H <= 2000)
                    {
                        HHonsu = 2;
                    }else if(2000 < OutputJSON.tOut.H && OutputJSON.tOut.H <= 3000){
                        HHonsu = 3;
                    }
                    else if (3000 < OutputJSON.tOut.H && OutputJSON.tOut.H <= 4000)
                    {
                        HHonsu = 4;
                    }
                    else if (4000 < OutputJSON.tOut.H && OutputJSON.tOut.H <= 5000)
                    {
                        HHonsu = 5;
                    }
                    else if (5000 < OutputJSON.tOut.H && OutputJSON.tOut.H <= 6000)
                    {
                        HHonsu = 6;
                    }
                    else if (6000 < OutputJSON.tOut.H && OutputJSON.tOut.H <= 7000)
                    {
                        HHonsu = 7;
                    }
                    else if (7000 < OutputJSON.tOut.H && OutputJSON.tOut.H <= 8000)
                    {
                        HHonsu = 8;
                    }else if (8000 < OutputJSON.tOut.H)
                    {
                        HHonsu = 9;
                    }
                    if(tIn.Kousi == "横格子ルーバー" || tIn.Kousi == "横格子面材Ａ" ){
                        if (3700 < OutputJSON.tOut.W && 2000 < OutputJSON.tOut.H){
                            HHonsu += 1;
                        }
                    }
                    HPitch = RoundDown((OutputJSON.tOut.H - (HariHasiIti * 2)) / (HHonsu - 1), 0);
                    HAmari = (OutputJSON.tOut.H - (HariHasiIti * 2)) - (HPitch * (HHonsu - 1));
                    if(2000 < lngL)
                    {
                        //７５はり
                        lngHariKanMin = 135;
                    }
                    else
                    {
                        //その他のはり
                        if (tIn.TateYoko == "縦格子")
                        {
                            lngHariKanMin = 64;
                        }
                        else
                        {
                            lngHariKanMin = 116;
                        }
                    }
                    lngHari = 0;
                    if (HPitch < lngHariKanMin)
                    {
                        //はり間最低寸法が取れない場合（このケースははり本数が2本の想定）
                        for (i = 1;i<= HHonsu;i++)
                        {
                            if (i == 1)
                            {
                                aryHari[lngHari].Top = RoundDown((OutputJSON.tOut.H - lngHariKanMin) / 2, 0);
                                aryHari[lngHari].Pitch = aryHari[lngHari].Top;
                            }
                            else
                            {
                                aryHari[lngHari].Top = aryHari[lngHari - 1].Top + lngHariKanMin;
                                aryHari[lngHari].Pitch = lngHariKanMin;
                            }
                            aryHari[lngHari].Height = OutputJSON.tOut.HMituke;
                            aryHari[lngHari].TargetNo = 1;
                            aryHari[lngHari].Top2 = aryHari[lngHari].Top;
                            lngHari = lngHari + 1;
                        }
                    }
                    else
                    {
                        //上下150ずつ取れる場合
                        for (i = 1;i<= HHonsu;i++)
                        {
                            k = HHonsu - i + 1;
                            aryHari[lngHari].Width = tIn.l - tIn.c - tIn.d;
                            aryHari[lngHari].Left = tIn.c;
                            if(i == 1)
                            {
                                if(HAmari > 0 && k <= HAmari){
                                    aryHari[lngHari].Pitch = HariHasiIti + 1;
                                }
                                else
                                {
                                    aryHari[lngHari].Pitch = HariHasiIti + 0;
                                }
                                aryHari[lngHari].Top = aryHari[lngHari].Pitch;

                            }
                            else {
                                if (HAmari > 0 && k <= HAmari){
                                    aryHari[lngHari].Pitch = lngHPitchW + 1;
                                }
                                else
                                {
                                    aryHari[lngHari].Pitch = lngHPitchW + 0;
                                }
                                aryHari[lngHari].Top = aryHari[lngHari - 1].Top + aryHari[lngHari].Pitch;
                            }
                            aryHari[lngHari].Height = OutputJSON.tOut.HMituke;
                            aryHari[lngHari].TargetNo = 1;
                            aryHari[lngHari].Top2 = aryHari[lngHari].Top;
                            lngHari = lngHari + 1;
                        }
                    }
                    //胴縁取付材分割
                    lngToritukezaiL = aryHari[1].Top;
                    lngDTZai = 0;
                    for (i = 2;i<= lngHari;i++)
                    {
                        lngHPitchW = aryHari[i].Top - aryHari[i - 1].Top;
                        if (lngToritukezaiL + HPitch <= DTZaiL)
                        {
                            lngToritukezaiL = lngToritukezaiL + HPitch;
                        }
                        else
                        {
                            //胴縁取付材を分割する
                            lngDTZai = lngDTZai + 1;
                            lngToritukezaiL = HPitch;
                        }
                    }
                    //最後の分
                    lngDTZai = lngDTZai + 1;
                    if(lngToritukezaiL + HariHasiIti > DTZaiL)
                    {
                        lngDTZai = lngDTZai + 1;
                    }
                    //判明した胴縁分割数を使って計算
                    ToritukezaiKousa = lngDTZai * Kousa;
                    DoubutiKousa = lngUnitSu * Kousa;
                    return (int)RoundUp((double)ToritukezaiKousa + (double)DoubutiKousa, 0);
                }
            }
        }
        //Ｗにより、はりを決定する
        private arytypHariSelect HariSelect(InputJSON tIn)
        {
            arytypHariSelect arytypHariSelect = new arytypHariSelect();
            int lngL;
            lngL = tIn.l - tIn.c - tIn.d;
            //はりを識別（Ｗによるデフォルト値)
            if(lngL <= 1200)
            {
                //基本は胴縁取付材
                if(tIn.TateYoko == "縦格子")
                {
                    if(tIn.Osamari == "独立")
                    {
                        arytypHariSelect.Hari = "５６はり";
                        arytypHariSelect.Mituke = 56;
                        arytypHariSelect.Mikomi = 56;
                        arytypHariSelect.DPitchMin = DoubutiPMinDKT;
                        arytypHariSelect.HPitchMin = 106;
                    }
                    else
                    {
                        arytypHariSelect.Hari = "胴縁取付材";
                        arytypHariSelect.Mituke = 56;
                        arytypHariSelect.Mikomi = 25;
                        arytypHariSelect.DPitchMin = DoubutiPMinDT;
                        arytypHariSelect.HPitchMin = 64;
                    }
                }
                else
                {
                    //横格子
                    if (tIn.Osamari == "独立")
                    {
                        arytypHariSelect.Hari = "５６はり";
                        arytypHariSelect.Mituke = 56;
                        arytypHariSelect.Mikomi = 56;
                        arytypHariSelect.DPitchMin = DoubutiPMinDKY;
                        arytypHariSelect.HPitchMin = 116;
                    }
                    else
                    {
                        arytypHariSelect.Hari = "胴縁取付材";
                        arytypHariSelect.Mituke = 56;
                        arytypHariSelect.Mikomi = 25;
                        arytypHariSelect.DPitchMin = DoubutiPMinDY;
                        arytypHariSelect.HPitchMin = 64;
                    }
                }
            }
            else if(1200 < lngL && lngL <= 2000){
                //基本は５６はり
                if (tIn.TateYoko == "縦格子")
                {
                    if (tIn.Osamari == "独立")
                    {
                        arytypHariSelect.Hari = "５６はり";
                        arytypHariSelect.Mituke = 56;
                        arytypHariSelect.Mikomi = 56;
                        arytypHariSelect.DPitchMin = DoubutiPMinDKT;
                        arytypHariSelect.HPitchMin = 106;
                    }
                    else
                    {
                        arytypHariSelect.Hari = "５６一体はり";
                        arytypHariSelect.Mituke = 56;
                        arytypHariSelect.Mikomi = 56;
                        arytypHariSelect.DPitchMin = DoubutiPMin56T;
                        arytypHariSelect.HPitchMin = 116;
                    }
                }
                else
                {
                    //横格子
                    if (tIn.Osamari == "独立")
                    {
                        arytypHariSelect.Hari = "５６はり";
                        arytypHariSelect.Mituke = 56;
                        arytypHariSelect.Mikomi = 56;
                        arytypHariSelect.DPitchMin = DoubutiPMinDKY;
                        arytypHariSelect.HPitchMin = 106;
                    }
                    else
                    {
                        arytypHariSelect.Hari = "５６はり";
                        arytypHariSelect.Mituke = 56;
                        arytypHariSelect.Mikomi = 56;
                        arytypHariSelect.DPitchMin = DoubutiPMin56Y;
                        arytypHariSelect.HPitchMin = 116;
                    }
                }
            }
            else if(2000 < lngL)
            {
                //基本は７５はり
                if (tIn.TateYoko == "縦格子")
                {
                    if (tIn.Osamari == "独立")
                    {
                        arytypHariSelect.Hari = "５６はり";
                        arytypHariSelect.Mituke = 56;
                        arytypHariSelect.Mikomi = 56;
                        arytypHariSelect.DPitchMin = DoubutiPMinDKT;
                        arytypHariSelect.HPitchMin = 106;
                    }
                    else
                    {
                        arytypHariSelect.Hari = "７５はり";
                        arytypHariSelect.Mituke = 75;
                        arytypHariSelect.Mikomi = 75;
                        arytypHariSelect.DPitchMin = DoubutiPMin75T;
                        arytypHariSelect.HPitchMin = 135;
                    }
                }
                else
                {
                    //横格子
                    if (tIn.Osamari == "独立")
                    {
                        arytypHariSelect.Hari = "５６はり";
                        arytypHariSelect.Mituke = 56;
                        arytypHariSelect.Mikomi = 56;
                        arytypHariSelect.DPitchMin = DoubutiPMinDKY;
                        arytypHariSelect.HPitchMin = 106;
                    }
                    else
                    {
                        arytypHariSelect.Hari = "７５はり";
                        arytypHariSelect.Mituke = 75;
                        arytypHariSelect.Mikomi = 75;
                        arytypHariSelect.DPitchMin = DoubutiPMin75Y;
                        arytypHariSelect.HPitchMin = 135;
                    }
                }
            }
            return arytypHariSelect;
        }

        //はりの計算を行う
        private OutputJSON SC03_HariCalc(InputJSON tIn, OutputJSON tOut)
        {
            OutputJSON OutputJSON = tOut;
            int lngL;                           //はり全体長
            int lngHPitchW;                     //はりピッチ（WK)
            int lngHAmari;                      //はり計算余り寸法
            int lngHHonsu = 0;                      //はり本数（ユニット毎）
            int lngUeNoH;                       //一段上ユニットのH寸法（はり間連結の2段目以降のはり位置計算で使用）
            int lngToritukezaiL;                //胴縁取付材長さ
            int lngToritukezaiS;                //胴縁取付材数
            int lngToritukezaiW;                //胴縁取付材長さ（WK）
            int lngToritukezaiA;                //胴縁取付材余り
            int lngDTZaiSum;                    //胴縁取付材長さ（合計）

            int lngHariLSu;                     //はり分割数
            int lngHariLHasu;                   //はり長さ端数
            int lngHariLSum;                    //はり長さ（合計）
            int lngHariLIdou;                   //はり長さ調整量
            int lngHariNokori;                  //はり長さ（残り）
            int lngHariKanMin = 0;                  //はり間の最低必要寸法

            int lngHariWidth;                   //はりの長さ（aryHariのWidthへ設定される値）
            int lngHariLeft;                    //はりの最左位置（aryHariのLeftへ設定される値）

            int i;
            int j;
            int k;
            int m;

            Boolean blnWK;
            //☆☆
            int lngH;
            int lngWK;
            if (tIn.Osamari == "独立")
            {
                //全体長設定
                //独立は本来胴縁取付材の全体長とはりの全体長は異なる（はりは柱で分割されるので）
                //いずれにしても長すぎて分割されることは無いので、ここは胴縁取付材の全体長を設定しておく
                lngL = tIn.l + OutputJSON.tOut.c + OutputJSON.tOut.d;
                //独立は５６はり
                OutputJSON.tOut.Hari = "５６はり";
                OutputJSON.tOut.HMituke = 56;
                OutputJSON.tOut.HMikomi = 56;
                OutputJSON.tOut.DbtiTop = 56;
                if (tIn.TateYoko == "縦格子")
                {
                    //縦格子は胴縁取付材の最低ピッチと合わせる
                    lngHariKanMin = DoubutiPMin56T;
                }
                else
                {
                    //横格子ははり間106以上
                    lngHariKanMin = 106;
                }
                //はり本数を識別（Ｈによるデフォルト値)
                if (tIn.TateYoko == "縦格子" || tIn.HMode == "はり前連結")
                {
                    lngH = tIn.H - tIn.SBSukima;
                    if (lngH <= 2000)
                    {
                        OutputJSON.tOut.HHonsu = 2;
                    }
                    else if (2000 < lngH && lngH <= 3000)
                    {
                        OutputJSON.tOut.HHonsu = 3;
                    }
                    else if (3000 < lngH && lngH <= 4000)
                    {
                        OutputJSON.tOut.HHonsu = 4;
                    }
                    else if (4000 < lngH && lngH <= 5000)
                    {
                        OutputJSON.tOut.HHonsu = 5;
                    }
                    else if (5000 < lngH && lngH <= 6000)
                    {
                        OutputJSON.tOut.HHonsu = 6;
                    }
                    else if (6000 < lngH && lngH <= 7000)
                    {
                        OutputJSON.tOut.HHonsu = 7;
                    }
                    else if (7000 < lngH && lngH <= 8000)
                    {
                        OutputJSON.tOut.HHonsu = 8;
                    }
                    else if (8000 < lngH)
                    {
                        OutputJSON.tOut.HHonsu = 9;
                    }
                }
                else
                {
                    //はり間連結の場合は胴縁取付材の分割に従うので、ここではゼロ
                    OutputJSON.tOut.HHonsu = 0;
                }
            }
            else
            {
                //壁内、持出し
                lngL = tIn.l - tIn.c - tIn.d;
                //はりを識別（Ｗによるデフォルト値)
                //☆☆
                if (lngL <= 1200)
                {
                    //縦・横格子ともに格子種類を問わず胴縁取付材（22/1/24仕様変更）
                    OutputJSON.tOut.Hari = "胴縁取付材";
                    OutputJSON.tOut.HMituke = 56;
                    OutputJSON.tOut.HMikomi = 25;
                    OutputJSON.tOut.DbtiTop = 0;
                    if (tIn.TateYoko == "縦格子")
                    {
                        lngHariKanMin = 64;
                    }
                    else
                    {
                        lngHariKanMin = 116;
                    }
                }
                else if (1200 < lngL && lngL <= 2000)
                {
                    if (tIn.TateYoko == "縦格子")
                    {
                        if (tIn.Osamari == "壁内")
                        {
                            //壁内・縦格子は５６一体はり
                            OutputJSON.tOut.Hari = "５６一体はり";
                            OutputJSON.tOut.HMituke = 56;
                            OutputJSON.tOut.HMikomi = 56;
                            OutputJSON.tOut.DbtiTop = 56;
                            lngHariKanMin = 64;
                        }
                        else
                        {
                            //持出し・縦格子は５６はり
                            OutputJSON.tOut.Hari = "５６はり";
                            OutputJSON.tOut.HMituke = 56;
                            OutputJSON.tOut.HMikomi = 56;
                            OutputJSON.tOut.DbtiTop = 56;
                            lngHariKanMin = 64;
                        }
                    }
                    else
                    {
                        //横格子は５６はり
                        OutputJSON.tOut.Hari = "５６はり";
                        OutputJSON.tOut.HMituke = 56;
                        OutputJSON.tOut.HMikomi = 56;
                        OutputJSON.tOut.DbtiTop = 56;
                        lngHariKanMin = 116;
                    }
                }
                else if (2000 < lngL)
                {
                    //７５はり
                    OutputJSON.tOut.Hari = "７５はり";
                    OutputJSON.tOut.HMituke = 75;
                    OutputJSON.tOut.HMikomi = 75;
                    OutputJSON.tOut.DbtiTop = 75;
                    lngHariKanMin = 135;
                }
                if (tIn.TateYoko == "縦格子" || tIn.HMode == "はり前連結")
                {
                    //はり本数を識別（Ｈによるデフォルト値)
                    if (OutputJSON.tOut.H <= 2000)
                    {
                        OutputJSON.tOut.HHonsu = 2;
                    }
                    else if (2000 < OutputJSON.tOut.H && OutputJSON.tOut.H <= 3000)
                    {
                        OutputJSON.tOut.HHonsu = 3;
                    }
                    else if (3000 < OutputJSON.tOut.H && OutputJSON.tOut.H <= 4000)
                    {
                        OutputJSON.tOut.HHonsu = 4;
                    }
                    else if (4000 < OutputJSON.tOut.H && OutputJSON.tOut.H <= 5000)
                    {
                        OutputJSON.tOut.HHonsu = 5;
                    }
                    else if (5000 < OutputJSON.tOut.H && OutputJSON.tOut.H <= 6000)
                    {
                        OutputJSON.tOut.HHonsu = 6;
                    }
                    else if (6000 < OutputJSON.tOut.H && OutputJSON.tOut.H <= 7000)
                    {
                        OutputJSON.tOut.HHonsu = 7;
                    }
                    else if (7000 < OutputJSON.tOut.H && OutputJSON.tOut.H <= 8000)
                    {
                        OutputJSON.tOut.HHonsu = 8;
                    }
                    else if (8000 < OutputJSON.tOut.H)
                    {
                        OutputJSON.tOut.HHonsu = 9;
                    }
                    if (tIn.Kousi == "横格子ルーバー" || tIn.Kousi == "横格子面材Ａ")
                    {
                        if (3700 < lngL && 2000 < OutputJSON.tOut.H)
                        {
                            OutputJSON.tOut.HHonsu = OutputJSON.tOut.HHonsu + 1;
                        }
                    }
                }
                else
                {
                    //はり間連結の場合は胴縁取付材の分割に従うので、ここではゼロ
                    OutputJSON.tOut.HHonsu = 0;
                }
            }
            //位置計算
            //持出しの場合はWの分割も考慮する必要があるが、現在は未実装
            if (tIn.TateYoko == "縦格子")
            {
                //縦格子の場合
                lngHPitchW = RoundDown(OutputJSON.tOut.H - (HariHasiIti * 2) / (OutputJSON.tOut.HHonsu - 1), 0);
                lngHAmari = (OutputJSON.tOut.H - (HariHasiIti * 2)) - (lngHPitchW * (OutputJSON.tOut.HHonsu - 1));
                for (j = 1; j <= OutputJSON.tOut.UnitSuH; j++)
                {
                    //その段の左端ユニットNo計算
                    k = (j - 1) * OutputJSON.tOut.UnitSuW + 1;
                    for (i = 1; i <= OutputJSON.aryUnits[k - 1].DoubutiSu; i++)
                    {
                        //☆☆
                        aryHari aryHari = new aryHari();
                        if (tIn.Osamari == "独立")
                        {
                            aryHari.Width = tIn.l - HasiraMituke(tIn);
                            aryHari.Left = HasiraMituke(tIn);
                        }
                        else
                        {
                            aryHari.Width = tIn.l - tIn.c - tIn.d;
                            aryHari.Left = tIn.c;
                        }
                        //はり位置とピッチは胴縁の情報を使う（同じ位置にあるので）
                        if (i == 1)
                        {
                            //一番上
                            aryHari.Top = OutputJSON.aryUnits[k - 1].YS;
                            aryHari.Pitch = OutputJSON.aryUnits[k - 1].YS;
                        }
                        else
                        {
                            //上から2番目以降のはりピッチはZL（上）⇒ZF（下）に向かって設定していく
                            m = 1;
                            if (OutputJSON.aryUnits[k - 1].ZL > 0)
                            {
                                m = m + 1;
                                if (i == m)
                                {
                                    aryHari.Top = OutputJSON.aryHaris[i - 1].Top + OutputJSON.aryUnits[k - 1].ZL;
                                    aryHari.Pitch = OutputJSON.aryUnits[k - 1].ZL;
                                }
                            }
                            if (OutputJSON.aryUnits[k - 1].ZI > 0)
                            {
                                m = m + 1;
                                if (i == m)
                                {
                                    aryHari.Top = OutputJSON.aryHaris[OutputJSON.aryHaris.Count - 1].Top + OutputJSON.aryUnits[k - 1].ZI;
                                    aryHari.Pitch = OutputJSON.aryUnits[k - 1].ZI;
                                }
                            }
                            if (OutputJSON.aryUnits[k - 1].ZG > 0)
                            {
                                m = m + 1;
                                if (i == m)
                                {
                                    aryHari.Top = OutputJSON.aryHaris[OutputJSON.aryHaris.Count - 1].Top + OutputJSON.aryUnits[k - 1].ZG;
                                    aryHari.Pitch = OutputJSON.aryUnits[k - 1].ZG;
                                }
                            }
                            if (OutputJSON.aryUnits[k - 1].ZF > 0)
                            {
                                m = m + 1;
                                if (i == m)
                                {
                                    aryHari.Top = OutputJSON.aryHaris[OutputJSON.aryHaris.Count - 1].Top + OutputJSON.aryUnits[k - 1].ZF;
                                    aryHari.Pitch = OutputJSON.aryUnits[k - 1].ZF;
                                }
                            }
                        }
                        aryHari.Height = OutputJSON.tOut.HMituke;
                        aryHari.Top2 = 0;
                        aryHari.TargetNo = j;
                        OutputJSON.aryHaris.Add(aryHari);
                    }
                }
                //胴縁取付材分割
                lngToritukezaiS = (int)RoundUp((double)lngL / (double)DTZaiL, 0);
                lngToritukezaiW = RoundDown(lngL / lngToritukezaiS, 0);
                //lngToritukezaiA = lngL - (lngToritukezaiS * lngToritukezaiW)   '縦格子の胴縁取付材は余りの分配不要
                lngToritukezaiL = 0;
                lngDTZaiSum = 0;
                for (i = 1; i <= lngToritukezaiS; i++)
                {
                    aryDTZai aryDTZai = new aryDTZai();
                    if (lngToritukezaiS == 1)
                    {
                        aryDTZai.l = lngL;
                    }
                    else if (i == 1 && lngToritukezaiS > 1)
                    {
                        lngToritukezaiL = lngL - DTZaiL * (lngToritukezaiS - 1);
                        if (lngToritukezaiL >= DTZaiLTMin)
                        {
                            //計算寸法が縦格子の胴縁取付材最小寸法以上ある
                            aryDTZai.l = lngToritukezaiL;
                            lngToritukezaiL = 0;
                        }
                        else
                        {
                            //計算寸法が縦格子の胴縁取付材最小寸法に満たない
                            aryDTZai.l = DTZaiLTMin;
                        }
                    }
                    else
                    {
                        if (lngToritukezaiL > 0)
                        {
                            //1番目の胴縁取付材を最小寸法に設定している場合は2番目でその分を調整
                            aryDTZai.l = DTZaiL - (DTZaiLTMin - lngToritukezaiL);
                            lngToritukezaiL = 0;
                        }
                        else
                        {
                            //通常時は4400
                            aryDTZai.l = DTZaiL;
                        }
                    }
                    aryDTZai.ZT = tIn.Mituke / 2;
                    aryDTZai.YT = tIn.Mituke / 2;
                    if (i == 1)
                    {
                        //最左
                        aryDTZai.ZT = OutputJSON.aryUnits[0].ZT;
                    }
                    if (i == lngToritukezaiS)
                    {
                        //最右
                        aryDTZai.YT = OutputJSON.aryUnits[OutputJSON.tOut.UnitSuW - 1].YT;
                    }
                    lngDTZaiSum = lngDTZaiSum + aryDTZai.l;
                    //格子と胴縁取付材の干渉チェック
                    aryDTZai.Kakou = KakouCheck(lngDTZaiSum, tIn, tOut); //Trueは加工OK、Falseは加工NG
                    OutputJSON.aryDTZais.Add(aryDTZai);

                }
                //結果設定
                OutputJSON.tOut.DTZaiSu = lngToritukezaiS;
            }
            else
            {
                //横格子の場合
                lngDTZaiSum = 0;
                if (tIn.HMode == "はり間連結")
                {
                    //胴縁取付材分割
                    //☆☆
                    if (tIn.Osamari == "独立")
                    {
                        lngToritukezaiS = (int)RoundUp((double)(OutputJSON.tOut.H - OutputJSON.tOut.SBSukima) / (double)tIn.DTZaiMaxH, 0);
                        lngToritukezaiW = RoundDown((OutputJSON.tOut.H - OutputJSON.tOut.SBSukima) / lngToritukezaiS, 0);
                        lngToritukezaiA = (OutputJSON.tOut.H - OutputJSON.tOut.SBSukima) - (lngToritukezaiS * lngToritukezaiW);
                    }
                    else
                    {
                        lngToritukezaiS = (int)RoundUp((double)OutputJSON.tOut.H / (double)tIn.DTZaiMaxH, 0);
                        lngToritukezaiW = RoundDown(OutputJSON.tOut.H / lngToritukezaiS, 0);
                        lngToritukezaiA = OutputJSON.tOut.H - (lngToritukezaiS * lngToritukezaiW);
                    }
                    for (i = 1; i <= lngToritukezaiS; i++)
                    {
                        k = lngToritukezaiS - i + 1;
                        aryDTZai aryDTZai = new aryDTZai();
                        if (lngToritukezaiA > 0 && k <= lngToritukezaiA)
                        {
                            aryDTZai.l = lngToritukezaiW + 1;
                        }
                        else
                        {
                            aryDTZai.l = lngToritukezaiW;
                        }
                        aryDTZai.ZT = 0;
                        aryDTZai.YT = 0;
                        lngDTZaiSum = lngDTZaiSum + aryDTZai.l;
                        //格子と胴縁取付材の干渉チェック
                        aryDTZai.Kakou = KakouCheck(lngDTZaiSum, tIn, tOut); //Trueは加工OK、Falseは加工NG
                        OutputJSON.aryDTZais.Add(aryDTZai);
                    }
                    //結果設定
                    OutputJSON.tOut.DTZaiSu = lngToritukezaiS;
                    //はり間連結の場合は、胴縁取付材毎に計算
                    lngUeNoH = 0;
                    for (i = 1; i <= OutputJSON.tOut.DTZaiSu; i++)
                    {
                        //前段までのHの合計計算
                        if (i > 1)
                        {
                            lngUeNoH = lngUeNoH + OutputJSON.aryDTZais[i - 1].l;
                        }
                        //はり本数を識別（胴縁取付材長さによるデフォルト値)
                        if (OutputJSON.aryDTZais[i].l <= 2000)
                        {
                            lngHHonsu = 2;
                        }
                        else if (2000 < OutputJSON.aryDTZais[i].l && OutputJSON.aryDTZais[i].l <= 3000)
                        {
                            lngHHonsu = 3;
                        }
                        else if (3000 < OutputJSON.aryDTZais[i].l && OutputJSON.aryDTZais[i].l <= 4000)
                        {
                            lngHHonsu = 4;
                        }
                        else if (4000 <= OutputJSON.aryDTZais[i].l && OutputJSON.aryDTZais[i].l <= 5000)
                        {
                            lngHHonsu = 5;
                        }
                        else if (5000 < OutputJSON.aryDTZais[i].l && OutputJSON.aryDTZais[i].l <= 6000)
                        {
                            lngHHonsu = 6;
                        }
                        else if (6000 < OutputJSON.aryDTZais[i].l && OutputJSON.aryDTZais[i].l <= 7000)
                        {
                            lngHHonsu = 7;
                        }
                        else if (7000 < OutputJSON.aryDTZais[i].l && OutputJSON.aryDTZais[i].l <= 8000)
                        {
                            lngHHonsu = 8;
                        }
                        else if (8000 < OutputJSON.aryDTZais[i].l)
                        {
                            lngHHonsu = 9;
                        }
                        //☆☆
                        if (tIn.Osamari != "独立")
                        {
                            if (tIn.Kousi == "横格子ルーバー" || tIn.Kousi == "横格子面材Ａ")
                            {
                                if (3700 < lngL && 2000 < OutputJSON.aryDTZais[i].l)
                                {
                                    lngHHonsu = lngHHonsu + 1;
                                }
                            }
                        }
                        OutputJSON.tOut.HHonsu = OutputJSON.tOut.HHonsu + lngHHonsu;
                        lngHPitchW = RoundDown((OutputJSON.aryDTZais[i].l - (HariHasiIti * 2)) / (lngHHonsu - 1), 0);
                        lngHAmari = (OutputJSON.aryDTZais[i].l - (HariHasiIti * 2)) - (lngHPitchW * (lngHHonsu - 1));
                        //●●
                        if (tIn.Osamari == "独立")
                        {
                            lngHariWidth = tIn.l - HasiraMituke(tIn);
                            lngHariLeft = HasiraMituke(tIn);
                        }
                        else
                        {
                            lngHariWidth = tIn.l - tIn.c - tIn.d;
                            lngHariLeft = tIn.c;
                        }
                        //はり位置計算（はり上下位置、はりピッチ計算、aryHari作成）
                        OutputJSON.aryHaris = YokoHariItiCalc(tIn.Osamari, tIn.Kousi, OutputJSON.aryDTZais[i].l, i, lngUeNoH, lngHHonsu, lngHPitchW, lngHariKanMin,
                                     OutputJSON.tOut.HMituke, lngHariWidth, lngHariLeft, OutputJSON.aryHaris, OutputJSON.aryHaris.Count);
                    }
                }
                else
                {
                    //はり前連結の場合は、全体Hで計算
                    //●●
                    if (tIn.Osamari == "独立")
                    {
                        lngHPitchW = RoundDown(((OutputJSON.tOut.H - OutputJSON.tOut.SBSukima) - (HariHasiIti * 2)) / (OutputJSON.tOut.HHonsu - 1), 0);
                        lngHAmari = ((OutputJSON.tOut.H - OutputJSON.tOut.SBSukima) - (HariHasiIti * 2)) - (lngHPitchW * (OutputJSON.tOut.HHonsu - 1));
                        lngHariWidth = tIn.l - HasiraMituke(tIn);
                        lngHariLeft = HasiraMituke(tIn);
                        lngWK = OutputJSON.tOut.H - OutputJSON.tOut.SBSukima;
                    }
                    else
                    {
                        lngHPitchW = RoundDown((OutputJSON.tOut.H - (HariHasiIti * 2)) / (OutputJSON.tOut.HHonsu - 1), 0);
                        lngHAmari = (OutputJSON.tOut.H - (HariHasiIti * 2)) - (lngHPitchW * (OutputJSON.tOut.HHonsu - 1));
                        lngHariWidth = tIn.l - tIn.c - tIn.d;
                        lngHariLeft = tIn.c;
                        lngWK = OutputJSON.tOut.H;
                    }
                    //はり位置計算（はり上下位置、はりピッチ計算、aryHari作成）
                    OutputJSON.aryHaris = YokoHariItiCalc(tIn.Osamari, tIn.Kousi, lngWK, 1, 0, OutputJSON.tOut.HHonsu, lngHPitchW, lngHariKanMin,
                                 OutputJSON.tOut.HMituke, lngHariWidth, lngHariLeft, OutputJSON.aryHaris, OutputJSON.aryHaris.Count);
                    //胴縁取付材分割
                    lngToritukezaiL = OutputJSON.aryHaris[1].Top;
                    for (i = 2; i < OutputJSON.aryHaris.Count; i++)
                    {
                        lngHPitchW = OutputJSON.aryHaris[i].Top - OutputJSON.aryHaris[i - 1].Top;
                        if (lngToritukezaiL + lngHPitchW <= DTZaiL)
                        {
                            lngToritukezaiL = lngToritukezaiL + lngHPitchW;
                        }
                        else
                        {
                            //胴縁取付材を分割する
                            aryDTZai aryDTZaiWK = new aryDTZai();
                            aryDTZaiWK.l = lngToritukezaiL;
                            aryDTZaiWK.ZT = 0;
                            aryDTZaiWK.YT = 0;
                            lngDTZaiSum = lngDTZaiSum + aryDTZaiWK.l;
                            //格子と胴縁取付材の干渉チェック
                            aryDTZaiWK.Kakou = KakouCheck(lngDTZaiSum, tIn, OutputJSON); //Trueは加工OK、Falseは加工NG
                            lngToritukezaiL = lngHPitchW;
                            OutputJSON.aryDTZais.Add(aryDTZaiWK);
                        }
                    }
                    //最後の分を作成
                    aryDTZai aryDTZai = new aryDTZai();
                    if (lngToritukezaiL + HariHasiIti > DTZaiL)
                    {
                        aryDTZai.l = lngToritukezaiL - lngHPitchW;
                        aryDTZai.ZT = 0;
                        aryDTZai.YT = 0;
                        lngDTZaiSum = lngDTZaiSum + aryDTZai.l;
                        //格子と胴縁取付材の干渉チェック
                        aryDTZai.Kakou = KakouCheck(lngDTZaiSum, tIn, OutputJSON); //Trueは加工OK、Falseは加工NG
                        OutputJSON.aryDTZais.Add(aryDTZai);
                        aryDTZai.l = lngHPitchW + HariHasiIti;
                        aryDTZai.ZT = 0;
                        aryDTZai.YT = 0;
                        lngDTZaiSum = lngDTZaiSum + aryDTZai.l;
                        //格子と胴縁取付材の干渉チェック
                        aryDTZai.Kakou = KakouCheck(lngDTZaiSum, tIn, OutputJSON); //Trueは加工OK、Falseは加工NG
                        OutputJSON.aryDTZais.Add(aryDTZai);
                    }
                    else
                    {
                        //☆☆
                        if (tIn.Osamari == "独立")
                        {
                            lngHPitchW = (OutputJSON.tOut.H - OutputJSON.tOut.SBSukima) - OutputJSON.aryHaris[OutputJSON.aryHaris.Count].Top;
                        }
                        else
                        {
                            lngHPitchW = OutputJSON.tOut.H - OutputJSON.aryHaris[OutputJSON.aryHaris.Count].Top;
                        }
                        aryDTZai.l = lngToritukezaiL + lngHPitchW;
                        aryDTZai.ZT = 0;
                        aryDTZai.YT = 0;
                        lngDTZaiSum = lngDTZaiSum + aryDTZai.l;
                        //格子と胴縁取付材の干渉チェック
                        aryDTZai.Kakou = KakouCheck(lngDTZaiSum, tIn, OutputJSON); //Trueは加工OK、Falseは加工NG
                        OutputJSON.aryDTZais.Add(aryDTZai);
                    }
                    //結果設定
                    OutputJSON.tOut.DTZaiSu = OutputJSON.aryDTZais.Count;
                }
            }
            //はりの横方向分割
            //寸法計算対象長さ計算（干渉が見つかった以前の合計）
            //☆☆
            if (tIn.Osamari == "独立")
            {
                //はりは柱間に付くのでそもそも柱見付を減算（* -1)、
                //その後はりの長さ計算でユニットWを加算していくがユニットWはそもそも柱面まであるのでその分も減算（* -1)
                //a,b寸法（壁～格子）は考慮しない
                lngHariNokori = (HasiraMituke(tIn) * -1) - OutputJSON.tOut.c - OutputJSON.tOut.d;
            }
            else
            {
                //壁内や持出しはユニットWがa寸法やc寸法、b寸法やd寸法を考慮済みなのではり計算ではゼロスタートで良いが
                //ユニットWとの差(はりがはみ出す部分)だけは考慮が必要
                //lngHariNokori = 0
                lngHariNokori = (OutputJSON.tOut.a - tIn.c) + (OutputJSON.tOut.b - tIn.d);
            }
            for (i = 0; i < OutputJSON.tOut.UnitSuW; i++)
            {
                //一番上のユニットだけ見る。ユニット情報からはり長さを計算※aryUnit(i).Widthは隙間込み
                lngHariNokori = lngHariNokori + OutputJSON.aryUnits[i].Width;
                //Debug.Print "i:" & i & "/aryUnit(i).Width:" & aryUnit(i).Width & "/aryUnit(i).MX:" & aryUnit(i).MX
            }
            lngHariLSu = (int)RoundUp((double)lngHariNokori / (double)HariL, 0);
            lngHariLHasu = lngHariNokori - (HariL * (lngHariLSu - 1));
            for (i = 1; i <= lngHariLSu; i++)
            {
                aryHariL aryHariL = new aryHariL();
                if (i == 1)
                {
                    aryHariL.Width = lngHariLHasu;
                }
                else if (i == 2)
                {
                    if (lngHariLHasu < HariLMin)
                    {
                        OutputJSON.aryHariLs[1].Width = HariLMin;
                        aryHariL.Width = HariL - (HariLMin - lngHariLHasu);
                    }
                    else
                    {
                        aryHariL.Width = HariL;
                    }
                }
                else
                {
                    aryHariL.Width = HariL;
                }
                OutputJSON.aryHariLs.Add(aryHariL);
            }
            //はりと胴縁の干渉をチェックしてはりの長さを調整する
            blnWK = false;
            while (blnWK == false)
            {
                lngHariLSum = 0;
                j = 0;
                if (OutputJSON.aryHariLs.Count > 1)
                {
                    //はりが分割されている場合に行う
                    for (i = 1; i < OutputJSON.aryHariLs.Count; i++)
                    {
                        lngHariLSum = lngHariLSum + OutputJSON.aryHariLs[i].Width;
                        lngHariLIdou = HariCheck(lngHariLSum, tIn, OutputJSON);
                        OutputJSON.aryHariLs[i].Width = OutputJSON.aryHariLs[i].Width + lngHariLIdou;
                        if (i < OutputJSON.aryHariLs.Count)
                        {
                            OutputJSON.aryHariLs[i + 1].Width = OutputJSON.aryHariLs[i + 1].Width - lngHariLIdou;
                        }
                        if (lngHariLIdou > 0)
                        {
                            j = i;
                            break;
                        }
                    }
                    if (j == 0)
                    {
                        //干渉が一つも無し
                        blnWK = true;
                    }
                    else
                    {
                        //干渉があったので、干渉が見つかった以前を再度切断し直し
                        blnWK = false;
                        //寸法計算対象長さ計算（干渉が見つかった以前の合計）
                        lngHariNokori = 0;
                        for (i = 1; i < j; i++)
                        {
                            //はりの長さを直接合計
                            lngHariNokori = lngHariNokori + OutputJSON.aryHariLs[i].Width;
                        }
                        lngHariLSu = (int)RoundUp((double)lngHariNokori / (double)HariL, 0);
                        lngHariLHasu = lngHariNokori - (HariL * (lngHariLSu - 1));
                        for (i = 1; i < lngHariLSu; i++)
                        {
                            if (i == 1)
                            {
                                if (lngHariLHasu < HariLMin)
                                {
                                    //最小寸法以下の場合
                                    OutputJSON.aryHariLs[i].Width = HariLMin;
                                }
                                else
                                {
                                    OutputJSON.aryHariLs[i].Width = lngHariLHasu;
                                }
                            }
                            else if (i == 2)
                            {
                                if (lngHariLHasu < HariLMin)
                                {
                                    OutputJSON.aryHariLs[i].Width = HariL - (HariLMin - lngHariLHasu);
                                }
                                else
                                {
                                    OutputJSON.aryHariLs[i].Width = HariL;
                                }
                            }
                            else
                            {
                                OutputJSON.aryHariLs[i].Width = HariL;
                            }
                        }
                    }
                }
                else
                {
                    blnWK = true;
                }
            }
            //はり全体長を計算
            //☆☆
            if (tIn.Osamari == "独立")
            {
                lngHariNokori = (HasiraMituke(tIn) * -1) - OutputJSON.tOut.c - OutputJSON.tOut.d;
            }
            else
            {
                //壁内や持出しはユニットWがa寸法やc寸法、b寸法やd寸法を考慮済みなのではり計算ではゼロスタートで良い
                lngHariNokori = 0;
                //lngHariNokori = (tOut.a - tIn.c) + (tOut.b - tIn.d)
            }
            for (i = 1; i < OutputJSON.tOut.UnitSuW; i++)
            {
                //一番上のユニットだけ見る。ユニット情報からはり長さを計算※aryUnit(i).Widthは隙間込み
                lngHariNokori = lngHariNokori + OutputJSON.aryUnits[i].Width;
            }
            //切断し直した結果、はり全体長が足りない場合は残り分を追加する
            lngHariLSum = 0;
            for (i = 0; i < OutputJSON.aryHariLs.Count; i++)
            {
                lngHariLSum = lngHariLSum + OutputJSON.aryHariLs[i].Width;
            }
            if (lngHariNokori > lngHariLSum)
            {
                OutputJSON.aryHariLs[OutputJSON.aryHariLs.Count - 1].Width = lngHariNokori - lngHariLSum;
            }
            return OutputJSON;
        }

        private OutputJSON SC04_HasiraCalc(InputJSON tIn, OutputJSON tOut)
        {
            int lngHHonsu;
            int lngHPitch;
            int lngHPitch1;
            int lngHPitch1Su;
            int lngHPitch2;
            int lngHPitch2Su;
            int lngHAmari;
            int lngPitch;
            int lngH;
            int lngDbtiIti;                 //格子左端を原点としたときの胴縁位置
            int lngHasiraIti;               //格子左端を原点としたときの柱芯位置（最左はｃ寸法）
            int lngKyori;                   //格子端部から柱芯までの距離
            int lngDbtiIdo;                 //胴縁移動距離
            int i;
            int j;
            int k;
            int l;
            int m;
            int n;
            int o;
            //柱ユニット作成
            OutputJSON OutputJSON = tOut;
            if (tIn.Osamari != "独立")
            {
                return null;
            }
            //※クランクになったら面毎に以下を実施。ｔOut.HasiraSyuは全体通しで設定する

            //柱本数計算
            lngHHonsu = (int)RoundUp(((double)tIn.l / (double)tIn.HPitch) + 1, 0);
            //柱ピッチ（調整前）計算
            lngHPitch = RoundDown(tIn.l / (lngHHonsu - 1), 0);
            //余り寸法計算（右ピッチへ割り当て）
            lngHAmari = tIn.l - (lngHPitch * (lngHHonsu - 1));
            //柱ピッチ設定（柱ピッチは2種類のみ）
            lngHPitch1 = lngHPitch;
            lngHPitch1Su = lngHHonsu - lngHAmari - 1;
            lngHPitch2 = 0;
            lngHPitch2Su = 0;
            if (lngHAmari > 0)
            {
                lngHPitch2 = lngHPitch + 1;
                lngHPitch2Su = lngHAmari;
            }
            //柱種計算
            //横格子ルーバー、横格子面材の場合、H2000を超えたら柱Ｂ、以外は柱Ａ
            //以外の格子は、柱ピッチから許容柱Ｈを計算（柱H=4/5*柱ピッチ）し、それを超えていたら柱Ｂ、以内なら柱Ａ
            if (tIn.Kousi == "横格子ルーバー" || tIn.Kousi == "横格子面材Ａ")
            {
                if (OutputJSON.tOut.H > 2000)
                {
                    OutputJSON.tOut.HasiraSyu = "柱Ｂ";
                }
                else
                {
                    OutputJSON.tOut.HasiraSyu = "柱Ａ";
                }
            }
            else
            {
                //大きいピッチを使う
                if (lngHPitch1 > lngHPitch2)
                {
                    lngPitch = lngHPitch1;
                }
                else
                {
                    lngPitch = lngHPitch2;
                }
                //判定（1000は切替部分の柱ピッチ、2000は切替部分の柱H）
                if (lngPitch <= 1000 || OutputJSON.tOut.H <= 2000)
                {
                    OutputJSON.tOut.HasiraSyu = "柱Ａ";
                }
                else
                {
                    //許容H計算
                    lngPitch = lngPitch - 1000;
                    if (lngPitch <= 0)
                    {
                        lngPitch = 0;
                    }
                    lngH = RoundDown((4 * lngPitch) / 5, 0) + 2000;
                    if (OutputJSON.tOut.H > lngH)
                    {
                        OutputJSON.tOut.HasiraSyu = "柱Ｂ";
                    }
                    else
                    {
                        OutputJSON.tOut.HasiraSyu = "柱Ａ";
                    }
                }
            }
            //柱データ作成
            // OutputJSON.aryHasiras
            aryHasira aryHasiraWK = new aryHasira();
            aryHasiraWK.No = OutputJSON.aryHasiras.Count + 1;
            aryHasiraWK.YS = -1;    //一番上のはりピッチ（150）
            aryHasiraWK.ZF = -1;    //１番目と２番目のはりピッチ
            aryHasiraWK.ZG = -1;    //２番目と３番目のはりピッチ
            aryHasiraWK.KZS = -1;   //一番下と格子端までのピッチ（150）
            aryHasiraWK.ZS = -1;    //一番下と格子端まで＋下端隙間＋柱埋め込み
            for (i = 1; i <= OutputJSON.aryHaris.Count; i++)
            {
                if (OutputJSON.aryHaris[i-1].TargetNo == 1)
                {
                    //独立は縦分割されない前提
                    switch (i)
                    {
                        case 1:
                            aryHasiraWK.YS = OutputJSON.aryHaris[i - 1].Pitch;
                            break;
                        case 2:
                            aryHasiraWK.ZF = OutputJSON.aryHaris[i - 1].Pitch;
                            break;
                        case 3:
                            aryHasiraWK.ZG = OutputJSON.aryHaris[i - 1].Pitch;
                            break;
                        default:
                            break;
                    }
                }
            }
            //一番下のはり
            aryHasiraWK.KZS = OutputJSON.tOut.H - OutputJSON.aryHaris[OutputJSON.aryHaris.Count - 1].Top - OutputJSON.tOut.SBSukima;
            aryHasiraWK.SBSukima = OutputJSON.tOut.SBSukima;
            aryHasiraWK.ZS = aryHasiraWK.KZS + aryHasiraWK.SBSukima + HasiraUmekomi;
            aryHasiraWK.HL = tIn.H + HasiraUmekomi;
            aryHasiraWK.HPitch1 = lngHPitch1;
            aryHasiraWK.HPitch1Su = lngHPitch1Su;
            if (lngHPitch2Su > 0)
            {
                aryHasiraWK.HPitch2 = lngHPitch2;
                aryHasiraWK.HPitch2Su = lngHPitch2Su;
            }
            else
            {
                //-1は表示しないの意味
                aryHasiraWK.HPitch2 = -1;
                aryHasiraWK.HPitch2Su = -1;
            }
            OutputJSON.aryHasiras.Add(aryHasiraWK);
            //柱位置と胴縁取付材位置の干渉チェック
            if (tIn.TateYoko == "横格子")
            {
                //横格子の場合のみ行う
                for (i = 1; i < OutputJSON.tOut.UnitSuW; i++)
                {
                    //一番上のユニットだけを見る（胴縁位置を知りたいだけなので）
                    //ユニット毎左端位置計算※原点は格子端
                    lngDbtiIti = OutputJSON.aryUnits[1].ZS;
                    if (i > 1)
                    {
                        //2ユニット目以降は一つ前までのユニットW（隙間、公差なし）を加算
                        for (k = 1; k <= i - 1; k++)
                        {
                            lngDbtiIti = lngDbtiIti + OutputJSON.aryUnits[k].Width;
                        }
                    }
                    //胴縁ピッチはMax6（YSを入れて）だが最後は見ない（胴縁本数自体は5本だから）
                    for (j = 1; j < 5; j++)
                    {
                        switch (j)
                        {
                            case 1:
                                //  lngDbtiIti = lngDbtiIti;
                                break;
                            case 2:
                                if (OutputJSON.aryUnits[i - 1].ZF > 0)
                                {
                                    //-1の場合があるので
                                    lngDbtiIti = lngDbtiIti + OutputJSON.aryUnits[i - 1].ZF;
                                }
                                break;
                            case 3:
                                if (OutputJSON.aryUnits[i - 1].ZG > 0)
                                {
                                    //-1の場合があるので
                                    lngDbtiIti = lngDbtiIti + OutputJSON.aryUnits[i - 1].ZG;
                                }
                                break;
                            case 4:
                                if (OutputJSON.aryUnits[i - 1].ZI > 0)
                                {
                                    //-1の場合があるので
                                    lngDbtiIti = lngDbtiIti + OutputJSON.aryUnits[i - 1].ZI;
                                }
                                break;
                            case 5:
                                if (OutputJSON.aryUnits[i - 1].ZL > 0)
                                {
                                    //-1の場合があるので
                                    lngDbtiIti = lngDbtiIti + OutputJSON.aryUnits[i - 1].ZL;
                                }
                                break;
                            default:
                                break;
                        }
                        //柱の位置を確認※原点位置は格子端
                        for (k = 1; k < OutputJSON.aryHasiras.Count; k++)
                        {
                            lngHasiraIti = tIn.c;   //最左の柱芯位置はc寸法
                            if (OutputJSON.aryHasiras[k-1].HPitch1Su > 0)
                            {
                                if (OutputJSON.aryHasiras[k - 1].HPitch2Su > 0)
                                {
                                    m = OutputJSON.aryHasiras[k - 1].HPitch1Su + OutputJSON.aryHasiras[k - 1].HPitch2Su;
                                }
                                else
                                {
                                    m = OutputJSON.aryHasiras[k - 1].HPitch1Su;
                                }
                            }
                            else
                            {
                                m = 0;
                            }
                            //l=0で先頭の柱を見る
                            for (l = 0; i < m; i++)
                            {
                                if (l > 0)
                                {
                                    if (l <= OutputJSON.aryHasiras[k - 1].HPitch1Su)
                                    {
                                        lngHasiraIti = lngHasiraIti + OutputJSON.aryHasiras[k - 1].HPitch1;
                                    }
                                    else
                                    {
                                        lngHasiraIti = lngHasiraIti + OutputJSON.aryHasiras[k - 1].HPitch2;
                                    }
                                }
                                if ((lngHasiraIti - HasiraKCheck) < lngDbtiIti && lngDbtiIti < (lngHasiraIti + HasiraKCheck))
                                {
                                    //干渉範囲にある場合は胴縁取付材の位置を移動する
                                    lngDbtiIdo = 0;
                                    //胴縁取付材は端部？中間？
                                    if (j == 1 || j == OutputJSON.aryUnits[i].DoubutiSu)
                                    {
                                        //両端部の胴縁取付材
                                        //この胴縁位置は左右格子端部どっちに近い？
                                        if (RoundDown(OutputJSON.aryUnits[i].MY / 2, 0) >= lngDbtiIti)
                                        {
                                            //左に近い
                                            lngKyori = tIn.c - tIn.a;            //格子端部～柱芯距離
                                            if (lngKyori <= 106)
                                            {
                                                //格子端部から離れる方向（右）に胴縁取付材を移動
                                                lngDbtiIdo = (HasiraKCheck - Math.Abs(lngDbtiIti - lngHasiraIti));
                                            }
                                            else
                                            {
                                                //格子端部に近づく方向（左）に胴縁取付材を移動
                                                lngDbtiIdo = (Math.Abs(lngDbtiIti - lngHasiraIti) + HasiraKCheck) * -1;
                                            }
                                        }
                                        else
                                        {
                                            //右に近い
                                            lngKyori = tIn.d - tIn.b;            //格子端部～柱芯距離
                                            if (lngKyori <= 106)
                                            {
                                                //格子端部から離れる方向（左）に胴縁取付材を移動
                                                lngDbtiIdo = (HasiraKCheck - Math.Abs(lngDbtiIti - lngHasiraIti)) * -1;
                                            }
                                            else
                                            {
                                                //格子端部に近づく方向（右）に胴縁取付材を移動
                                                lngDbtiIdo = (Math.Abs(lngDbtiIti - lngHasiraIti) + HasiraKCheck);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //中間の胴縁取付材
                                        if (lngDbtiIti <= lngHasiraIti)
                                        {
                                            //柱と胴縁が同じ位置、あるいは、胴縁が手前（胴縁位置＜＝柱位置）
                                            //左に胴縁取付材を移動
                                            lngDbtiIdo = (HasiraKCheck - Math.Abs(lngDbtiIti - lngHasiraIti)) * -1;
                                        }
                                        else
                                        {
                                            //柱が手前（柱位置＜胴縁位置）
                                            //右に胴縁取付材を移動
                                            lngDbtiIdo = (HasiraKCheck - Math.Abs(lngDbtiIti - lngHasiraIti));
                                        }
                                    }
                                    //胴縁取付材を移動する
                                    if (lngDbtiIdo != 0)
                                    {
                                        lngDbtiIti = lngDbtiIti + lngDbtiIdo;
                                        switch (j)
                                        {
                                            case 1:
                                                OutputJSON.aryUnits[i].ZS = OutputJSON.aryUnits[i].ZS + lngDbtiIdo;
                                                OutputJSON.aryUnits[i].ZF = OutputJSON.aryUnits[i].ZF - lngDbtiIdo;
                                                lngDbtiIdo = 0;
                                                break;
                                            case 2:
                                                if (OutputJSON.aryUnits[i].ZF > 0)
                                                {
                                                    //-1の場合があるので
                                                    OutputJSON.aryUnits[i].ZF = OutputJSON.aryUnits[i].ZF + lngDbtiIdo;
                                                    if (OutputJSON.aryUnits[i].ZG > 0)
                                                    {
                                                        OutputJSON.aryUnits[i].ZG = OutputJSON.aryUnits[i].ZG - lngDbtiIdo;
                                                    }
                                                    else
                                                    {
                                                        //ZGが使われていない場合、YSを調整する
                                                        OutputJSON.aryUnits[i].YS = OutputJSON.aryUnits[i].YS - lngDbtiIdo;
                                                    }
                                                    lngDbtiIdo = 0;
                                                }
                                                break;
                                            case 3:
                                                if (OutputJSON.aryUnits[i].ZG > 0)
                                                {
                                                    //-1の場合があるので
                                                    OutputJSON.aryUnits[i].ZG = OutputJSON.aryUnits[i].ZG + lngDbtiIdo;
                                                    if (OutputJSON.aryUnits[i].ZI > 0)
                                                    {
                                                        OutputJSON.aryUnits[i].ZI = OutputJSON.aryUnits[i].ZI - lngDbtiIdo;
                                                    }
                                                    else
                                                    {
                                                        //ZGが使われていない場合、YSを調整する
                                                        OutputJSON.aryUnits[i].YS = OutputJSON.aryUnits[i].YS - lngDbtiIdo;
                                                    }
                                                    lngDbtiIdo = 0;
                                                }
                                                break;
                                            case 4:
                                                if (OutputJSON.aryUnits[i].ZI > 0)
                                                {
                                                    //-1の場合があるので
                                                    OutputJSON.aryUnits[i].ZI = OutputJSON.aryUnits[i].ZI + lngDbtiIdo;
                                                    if (OutputJSON.aryUnits[i].ZL > 0)
                                                    {
                                                        OutputJSON.aryUnits[i].ZL = OutputJSON.aryUnits[i].ZL - lngDbtiIdo;
                                                    }
                                                    else
                                                    {
                                                        //ZGが使われていない場合、YSを調整する
                                                        OutputJSON.aryUnits[i].YS = OutputJSON.aryUnits[i].YS - lngDbtiIdo;
                                                    }
                                                    lngDbtiIdo = 0;
                                                }
                                                break;
                                            case 5:
                                                if (OutputJSON.aryUnits[i].ZL > 0)
                                                {
                                                    //-1の場合があるので
                                                    OutputJSON.aryUnits[i].ZL = OutputJSON.aryUnits[i].ZL + lngDbtiIdo;
                                                    OutputJSON.aryUnits[i].YS = OutputJSON.aryUnits[i].YS - lngDbtiIdo;
                                                    lngDbtiIdo = 0;
                                                }
                                                break;
                                            default:
                                                break;
                                        }
                                        //縦方向ユニットにも反映
                                        for (n = 2; n < OutputJSON.tOut.UnitSuH; n++)
                                        {
                                            //対応するユニット
                                            o = i + ((n - 1) * OutputJSON.tOut.UnitSuW);
                                            OutputJSON.aryUnits[o].ZS = OutputJSON.aryUnits[i].ZS;
                                            OutputJSON.aryUnits[o].ZF = OutputJSON.aryUnits[i].ZF;
                                            OutputJSON.aryUnits[o].ZG = OutputJSON.aryUnits[i].ZG;
                                            OutputJSON.aryUnits[o].ZI = OutputJSON.aryUnits[i].ZI;
                                            OutputJSON.aryUnits[o].ZL = OutputJSON.aryUnits[i].ZL;
                                            OutputJSON.aryUnits[o].YS = OutputJSON.aryUnits[i].YS;
                                        }
                                    }
                                }
                            }

                        }
                    }
                }
            }
            return OutputJSON;
        }

        //◎◎面全体の柱見付算出（独立納まり専用)
        private int HasiraMituke(InputJSON tIn)
        {
            int lngW = 0;
            if (tIn.LDeiri == "出隅" || tIn.LDeiri == "入隅")
            {
                lngW = lngW + (HasiraMitukeC / 2);
            }
            else
            {
                lngW = lngW + (HasiraMitukeN / 2);
            }
            if (tIn.RDeiri == "出隅" || tIn.RDeiri == "入隅")
            {
                lngW = lngW + (HasiraMitukeC / 2);
            }
            else
            {
                lngW = lngW + (HasiraMitukeN / 2);
            }
            return lngW;
        }


        //胴縁分割位置が格子位置と干渉するかをチェック
        private Boolean KakouCheck(int l, InputJSON tIn, OutputJSON OutputJSON)
        {
            int i;
            int j;
            int k;
            int Kiti;               //格子芯位置
            int Ksu;                //格子本数
            int Kpitch;             //格子ピッチ
            int Iti;                //胴縁取付材加工位置
            int CHKHABA = 10;
            Boolean KakouCheck = true;//加工OK
            if (tIn.TateYoko == "縦格子")
            {
                Kiti = OutputJSON.aryUnits[1].ZT;    //ZTは格子芯から胴縁左端
                for (i = 1; i < OutputJSON.tOut.UnitSuW; i++)
                {
                    //一番上のユニットしか見ない（格子位置も胴縁分割位置も縦方向ユニットは全て共通だから）
                    if (i > 1)
                    {
                        //2番目以降のユニットは公差も含めないと胴縁取付材位置と比較出来ない
                        Kiti = Kiti + OutputJSON.aryUnits[i].Kousa;
                    }
                    Ksu = 0;
                    //For k = 0 To aryUnit(i).KA + aryUnit(i).KB
                    if (OutputJSON.aryUnits[i].KA > 0)
                    {
                        if (OutputJSON.aryUnits[i].KB > 0)
                        {
                            j = OutputJSON.aryUnits[i].KA + OutputJSON.aryUnits[i].KB;
                        }
                        else
                        {
                            j = OutputJSON.aryUnits[i].KA;
                        }
                    }
                    else
                    {
                        if (OutputJSON.aryUnits[i].KB > 0)
                        {
                            j = OutputJSON.aryUnits[i].KB;
                        }
                        else
                        {
                            j = 0;
                        }
                    }
                    if (i == 1)
                    {
                        j = j - 1;   //先頭ユニットは1本目がZTなのでループ回数を1減らす
                    }
                    for (k = 1; k < j; k++)
                    {
                        //一番左端の格子は見ない
                        //格子面位置計算
                        Ksu = Ksu + 1;
                        if (Ksu > OutputJSON.aryUnits[i].KA)
                        {
                            //ピッチ2を使う格子
                            Kpitch = OutputJSON.aryUnits[i].ZN;
                            Kiti = Kiti + OutputJSON.aryUnits[i].ZN;
                        }
                        else
                        {
                            //ピッチ1を使う格子
                            Kpitch = OutputJSON.aryUnits[i].ZM;
                            Kiti = Kiti + OutputJSON.aryUnits[i].ZM;
                        }
                        //胴縁取付材加工位置は1ユニット内に2カ所（両端格子と隣の格子の中間位置）のみ
                        //k = IIf(i = 1, 1, 2) Or k = j
                        if ((i == 1 || i == 2) && k == i || k == j)
                        {
                            //一番目のユニットはK=1が2本目、以降のユニットはK=2が2本目
                            //最初の格子と2本目の格子の中間、または、最後の格子と1本目前の格子の中間に胴縁取付材加工位置がある
                            //胴縁取付材加工位置
                            Iti = Kiti - RoundDown(Kpitch / 2, 0);
                            //Debug.Print "i:" & i & "/aryUnit(i).Kousa:" & aryUnit(i).Kousa & "/Kpitch:" & Kpitch & "/Kiti:" & Kiti & "/Iti:" & Iti & "/(Iti - CHKHABA):" & Iti - CHKHABA & "/L:" & l & "/(Iti + CHKHABA):" & Iti + CHKHABA
                            if ((Iti - CHKHABA) <= l && l <= (Iti + CHKHABA))
                            {
                                KakouCheck = false;  //加工NG
                                return KakouCheck;
                            }
                        }
                    }
                }
            }
            else
            {
                //横格子の場合
                Kiti = OutputJSON.aryUnits[1].ZT;
                for (i = 1; i <= OutputJSON.tOut.UnitSuH; i++)
                {
                    //一番左端のユニットIDX計算
                    j = (i - 1) * OutputJSON.tOut.UnitSuW + 1;
                    Ksu = 0;
                    for (k = 1; k < OutputJSON.aryUnits[j].KA + OutputJSON.aryUnits[j].KB; k++)
                    {
                        //格子上面位置計算※一番上の格子は見ない
                        Ksu = Ksu + 1;
                        if (Ksu > OutputJSON.aryUnits[j].KA)
                        {
                            Kiti = Kiti + OutputJSON.aryUnits[j].Pitch2;
                        }
                        else
                        {
                            Kiti = Kiti + OutputJSON.aryUnits[j].Pitch1; ;
                        }
                        Iti = Kiti - (tIn.Mituke / 2);
                        //Debug.Print "L:" & l & "/Iti:" & Iti & "/CHKHABA" & CHKHABA
                        if ((Iti - CHKHABA) <= l && l <= Iti)
                        {
                            KakouCheck = false;  //加工NG
                            //Debug.Print "OUT!! L:" & l & "/Iti:" & Iti & "/CHKHABA" & CHKHABA
                            return KakouCheck;
                        }
                    }
                }
            }
            return KakouCheck;
        }

        //横格子用はり位置計算（aryHari作成）
        //Osamari        ：納まり（壁内/持出し/独立）
        //Kousi          ：格子（２０×３０格子/３０×５０格子/５０×５０格子/クリア格子/エコリルウッド格子/横格子ルーバー/横格子面材Ａ）
        //HSize          ：計算対象のH寸法（はり間は胴縁取付材L、はり前は全体H）
        //lngUnitNo      ：計算対象格子ユニットのNo（はり前の場合は1固定）
        //lngUeNoH       ：計算対象格子ユニット以外の上にある格子ユニットHの合計（はり前の場合はゼロ固定）
        //lngHHonsu      ：はり本数
        //lngHPitchW     ：はりピッチ※HSizeをはり本数で割ったもの
        //lngHariKanMin  ：はりピッチの最小寸法
        //HariMituke     ：はりの見付
        //HariWidth      ：はりの全体長
        //HariLeft       ：はりの最左位置
        //※aryHariははりの本数分作成される。
        //  Lが長くてはりが切断される場合、その情報はaryHariLで管理し、aryHariでは管理されな
        private List<aryHari> YokoHariItiCalc(string Osamari, string Kousi, int HSize, int lngUnitNo, int lngUeNoH,
                            int lngHHonsu, int lngHPitchW, int lngHariKanMin, int HariMituke, int HariWidth, int HariLeft, List<aryHari> aryHari, int lngHari)
        {
            List<aryHari> aryHaris = new List<aryHari>();

            int lngH; //計算ワーク
            int lngHariUMin; //はり上最小寸法
            int lngPitch; //はりピッチ
            int lngPitchU; //上ピッチ
            int lngPitchD; //下ピッチ
            int lngAmari; //余り
            int lngAmariK; //余り加算値
            int i;

            //ケース分け
            //ケース１：はり上、はり下、はりピッチのいずれも150取れる場合
            //    ⇒はり上､はり下は150､残りをはりピッチで均等割りし､余りははりピッチのみへ下から1mmずつ割り振る
            //ケース２：３等分した結果Ｘが、はりピッチ最小寸法＜Ｘ＜＝１５０。（例：150＋150＋149）この場合はりは2本。
            //    ⇒はり下：150、はりピッチ：150、はり上を一番小さくする（はり上：149）
            //ケース３：３等分した結果Ｘが、はりピッチ最小寸法＜＝Ｘ＜１５０。（例：106＋106＋107）この場合はりは2本。
            //    ⇒はり下：はりピッチ最小寸法＋余りを均等割り（上よりも大）107、はりピッチ：はりピッチ最小寸法106、はり上：はりピッチ最小寸法＋余りを均等割り（下よりも小）106
            //ケース４：Ｈ＞＝はりピッチ最小寸法＋はり上最小寸法×２。（例：43＋106＋44）この場合はりは2本。
            //    ⇒はり下：はり上最小寸法＋余りを均等割り（上よりも大）44、はりピッチ：はりピッチ最小寸法106、はり上：はり上最小寸法＋余りを均等割り（下よりも小）43
            //ケース５：Ｈ＜はりピッチ最小寸法＋はり上最小寸法×２。（例：43＋106＋28）この場合はりは2本で独立のみ発生。
            //    ⇒はり上：はり上最小寸法43、はりピッチ：はりピッチ最小寸法106、はり下：残り28

            //はり上最小寸法設定
            if (Osamari == "独立")
            {
                //独立は５６はり固定
                if (Kousi == "２０×３０格子" || Kousi == "３０×５０格子" || Kousi == "エコリルウッド格子")
                {
                    lngHariUMin = 43;
                }
                else if (Kousi == "５０×５０格子" || Kousi == "クリア格子")
                {
                    lngHariUMin = 43;
                }
                else if (Kousi == "横格子ルーバー")
                {
                    lngHariUMin = 64;
                }
                else
                {
                    //横格子面材Ａ
                    lngHariUMin = 122;
                }
            }
            else
            {
                if (HariMituke == 75)
                {
                    //７５はり
                    if (Kousi == "２０×３０格子")
                    {
                        lngHariUMin = 38;
                    }
                    else if (Kousi == "３０×５０格子" || Kousi == "エコリルウッド格子")
                    {
                        lngHariUMin = 38;
                    }
                    else if (Kousi == "５０×５０格子")
                    {
                        lngHariUMin = 38;
                    }
                    else if (Kousi == "クリア格子")
                    {
                        lngHariUMin = 38;
                    }
                    else if (Kousi == "横格子ルーバー")
                    {
                        lngHariUMin = 50;
                    }
                    else
                    {
                        //横格子面材Ａ
                        lngHariUMin = 115;
                    }
                }
                else
                {
                    //５６はり
                    if (Kousi == "２０×３０格子")
                    {
                        lngHariUMin = 28;
                    }
                    else if (Kousi == "３０×５０格子" || Kousi == "エコリルウッド格子")
                    {
                        lngHariUMin = 28;
                    }
                    else if (Kousi == "５０×５０格子")
                    {
                        lngHariUMin = 37;
                    }
                    else if (Kousi == "クリア格子")
                    {
                        lngHariUMin = 29;
                    }
                    else if (Kousi == "横格子ルーバー")
                    {
                        lngHariUMin = 50;
                    }
                    else
                    {
                        //横格子面材Ａ
                        lngHariUMin = 122;
                    }
                }
            }
            //ケース判定
            lngH = (lngHHonsu - 1) * 150 + 300;
            if (lngH <= HSize)
            {
                //ケース１
                lngPitchU = 150;
                lngPitchD = 150;
                lngPitch = RoundDown((HSize - 300) / (lngHHonsu - 1), 0);
                lngAmari = HSize - 300 - (lngPitch * (lngHHonsu - 1));
            }
            else
            {
                lngH = RoundDown(HSize / 3, 0);
                if (lngHariKanMin < lngH && lngH <= 150)
                {
                    //ケース２
                    lngAmari = HSize - (lngH * 3);
                    lngPitchU = lngH;
                    if (lngAmari > 1)
                    {
                        lngPitch = lngH + 1;
                    }
                    else
                    {
                        lngPitch = lngH;
                    }
                    if (lngAmari > 0)
                    {
                        lngPitchD = lngH + 1;
                    }
                    else
                    {
                        lngPitchD = lngH;
                    }
                    lngAmari = 0;
                }
                else if (lngHariKanMin <= lngH && lngH < 150)
                {
                    //ケース３
                    lngAmari = HSize - (lngH * 3);
                    lngPitchU = lngH;
                    if (lngAmari > 1)
                    {
                        lngPitch = lngH + 1;
                    }
                    else
                    {
                        lngPitch = lngH;
                    }
                    if (lngAmari > 0)
                    {
                        lngPitchD = lngH + 1;
                    }
                    else
                    {
                        lngPitchD = lngH;
                    }
                    lngAmari = 0;
                }
                else
                {
                    lngH = lngHariKanMin + (lngHariUMin * 2);
                    if (HSize >= lngH)
                    {
                        //ケース４
                        lngPitchD = (int)RoundUp((double)(HSize - lngHariKanMin) / 2, 0);
                        lngPitch = lngHariKanMin;
                        lngPitchU = HSize - lngPitchD - lngPitch;
                        lngAmari = 0;
                    }
                    else
                    {
                        //ケース５（独立のみあり得る）
                        lngPitchU = lngHariUMin;
                        lngPitch = lngHariKanMin;
                        lngPitchD = HSize - lngPitchU - lngPitch;
                        lngAmari = 0;
                    }
                }
            }
            //はり情報作成（はり本数分作成）
            //はりは本数分作成するので、はり下150などの分は作成されない。つまりlngPitchDは設定先がない。
            for (i = 1; i < lngHHonsu; i++)
            {
                lngHari = lngHari + 1;
                aryHari aryHariWK = new aryHari();
                //余り加算値計算
                lngAmariK = 0;
                if (lngAmari > 0)
                {
                    //はりピッチのみの下から余りを吸収
                    //HHonsu=2/Amari=1 i=1:×,i=2:○(,i=3:×)
                    //HHonsu=3/Amari=1 i=1:×,i=2:×,i=3:○(,i=4:×)
                    //HHonsu=3/Amari=2 i=1:×,i=2:○,i=3:○(,i=4:×)
                    if (lngHHonsu - lngAmari < i)
                    {
                        lngAmariK = 1;
                    }
                }
                //はり情報設定
                //共通項目設定
                aryHariWK.Width = HariWidth;
                aryHariWK.Left = HariLeft;
                aryHariWK.Height = HariMituke;
                if (i == 1)
                {
                    //一番上のはり
                    aryHariWK.Top = lngUeNoH + lngPitchU;
                    aryHariWK.Top2 = lngPitchU;
                    aryHariWK.TargetNo = lngUnitNo;
                    aryHariWK.Pitch = lngPitchU;
                    aryHaris.Add(aryHariWK);
                }
                else
                {
                    //各ユニットの２番目以降のはり
                    //※一番下のはりから格子下端部までの分はデータは存在しない（画面表示時に逆算してピッチ計算している）
                    aryHariWK.Top = aryHari[lngHari - 1].Top + lngPitch + lngAmariK;
                    aryHariWK.Top2 = aryHari[lngHari - 1].Top2 + lngPitch + lngAmariK;
                    aryHariWK.TargetNo = lngUnitNo;
                    aryHariWK.Pitch = lngPitch + lngAmariK;
                    aryHaris.Add(aryHariWK);
                }
            }
            return aryHaris;
        }

        //はり分割位置が胴縁位置と干渉するかをチェック
        private int HariCheck(int l, InputJSON tIn, OutputJSON OutputJSON)
        {
            int i;
            int j;
            int k;
            int lngDbtiIti;             //胴縁芯位置
            int HariCheck = 0;          //調整なし（干渉しない）
            for (i = 1; i <= OutputJSON.tOut.UnitSuW; i++)
            {
                //一番上のユニットだけを見る（胴縁位置を知りたいだけなので）
                //ユニット毎左端位置計算
                lngDbtiIti = tIn.YSukima / 2;   //先頭の隙間
                if (i > 1)
                {
                    for (k = 0; k < i - 1; k++)
                    {
                        lngDbtiIti = lngDbtiIti + OutputJSON.aryUnits[k].Width;
                    }
                }
                //胴縁ピッチはMax6だが最後は見ない（最後YSは150固定だから）
                for (j = 0; j < 5; j++)
                {
                    switch (j)
                    {
                        case 0:
                            lngDbtiIti = lngDbtiIti + OutputJSON.aryUnits[i].ZS;
                            break;
                        case 1:
                            if (OutputJSON.aryUnits[i].ZF > 0)
                            {
                                //-1の場合があるので
                                lngDbtiIti = lngDbtiIti + OutputJSON.aryUnits[i].ZF;
                            }
                            break;
                        case 2:
                            if (OutputJSON.aryUnits[i].ZG > 0)
                            {
                                //-1の場合があるので
                                lngDbtiIti = lngDbtiIti + OutputJSON.aryUnits[i].ZG;
                            }
                            break;
                        case 3:
                            if (OutputJSON.aryUnits[i].ZI > 0)
                            {
                                //-1の場合があるので
                                lngDbtiIti = lngDbtiIti + OutputJSON.aryUnits[i].ZI;
                            }
                            break;
                        case 4:
                            if (OutputJSON.aryUnits[i].ZL > 0)
                            {
                                //-1の場合があるので
                                lngDbtiIti = lngDbtiIti + OutputJSON.aryUnits[i].ZL;
                            }
                            break;
                        default:
                            break;
                    }
                    //Debug.Print "L:" & l & "/Iti:" & lngDbtiIti & "/HariIdou:" & HariIdou
                    if ((lngDbtiIti - HariIdou) <= l && l <= (lngDbtiIti + HariIdou))
                    {
                        //干渉範囲にある
                        HariCheck = HariIdou + (lngDbtiIti - l);
                        return HariCheck;
                    }
                }
            }
            return HariCheck;
        }
        private InputJSON JSONSet()
        {
            InputJSON InputJSON = new InputJSON();
            InputJSON.a = 0;
            InputJSON.b = 2;
            InputJSON.c = 175;
            InputJSON.d = 32;
            InputJSON.DTZaiMaxH = 4400;
            InputJSON.H = 2450;
            InputJSON.HMode = "";
            InputJSON.HPitch = 2000;
            InputJSON.KCorner = "";
            InputJSON.Kousi = "５０×５０格子";
            InputJSON.Kpitch = 100;
            InputJSON.KPitchText = "";
            InputJSON.l = 1450;
            InputJSON.LDeiri = "";
            InputJSON.LKatiMake = "";
            InputJSON.MaxH = 5000;
            InputJSON.MaxW = 835;
            InputJSON.Mikomi = 50;
            InputJSON.Mituke = 50;
            InputJSON.Osamari = "独立";
            InputJSON.RDeiri = "";
            InputJSON.Renketu = "";
            InputJSON.RKatiMake = "";
            InputJSON.SBSukima = 50;
            InputJSON.TateYoko = "縦格子";
            InputJSON.TSukima = 6;
            InputJSON.TWait = 550;
            InputJSON.YMode = "出来寸優先";
            InputJSON.YSukima = 6;
            return InputJSON;
        }
    }
}
