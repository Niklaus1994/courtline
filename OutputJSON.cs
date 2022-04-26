using System;
using System.Collections.Generic;

namespace courtline
{
    public class OutputJSON
    {
        public Result Result { get; set; }
        public tOut tOut { get; set; }
        public List<aryUnit> aryUnits { get; set; }
        public List<aryHari> aryHaris { get; set; }
        public List<aryHariL> aryHariLs { get; set; }
        public List<aryDTZai> aryDTZais { get; set; }
        public List<aryDTZaiL> aryDTZaiLs { get; set; }
        public List<aryHasira> aryHasiras { get; set; }
    }
    //ステータスブロック
    public class Result
    {
        public string status { get; set; }           //"OK"、"NG"
        public string massage { get; set; }          //エラー内容メッセージ。statusが"OK"の場合は空文字。"NG"の場合、エラー理由を返却。
    }
    //基本情報出力ブロック
    public class tOut
    {
        public int W { get; set; }                   //横格子：総格子Ｗ、縦格子：胴縁Ｗ
        public int H { get; set; }                   //横格子：胴縁Ｌ、縦格子：総格子Ｌ
        public int KHonsu { get; set; }              //格子本数
        public int KPitch1 { get; set; }             //格子ピッチ1
        public int KPitch1Su { get; set; }           //格子ピッチ1数
        public int KPitch2 { get; set; }             //格子ピッチ2
        public int KPitch2Su { get; set; }           //格子ピッチ2数
        public int KPitchSu { get; set; }            //格子ピッチ数（合計）
        public int UnitSuW { get; set; }             //横方向ユニット数
        public int UnitSuH { get; set; }             //縦方向ユニット数
        public int a { get; set; }                   //a寸法（縦格子ピッチ優先時再計算結果）
        public int b { get; set; }                   //b寸法（縦格子ピッチ優先時再計算結果）
        public int c { get; set; }                   //c寸法
        public int d { get; set; }                   //d寸法
        public int Sa { get; set; }                  //縦格子ピッチ優先時のＷとの差
        public string Hari { get; set; }             //はり種類
        public int HHonsu { get; set; }              //はり本数（縦方向で何本はりが必要になるか）
        public int HHonsuW { get; set; }             //はりＷ方向分割数（4400を超えた場合は分割が必要。内観右から4400ずつ、最左は残り）
        public int HMituke { get; set; }             //はり見付寸法
        public int HMikomi { get; set; }             //はり見込寸法
        public int DbtiTop { get; set; }             //はりから胴縁上端位置までの距離（平面図描画時専用）
        public int DTZaiSu { get; set; }             //胴縁取付材分割数（縦格子はW方向、横格子はH方向※実際の胴縁数は考慮されない＝aryDbtiToritukeZaiの配列数）
        public int ReCalcCnt1 { get; set; }          //格子ユニット希望サイズオーバーで再計算が行われた回数
        public int ReCalcCnt2 { get; set; }          //重量制限で再計算が行われた回数
        public int ReCalcCnt3 { get; set; }          //格子本数下限未満で再計算が行われた回数
        public int MaxW { get; set; }                //希望分割寸法Ｗ（格子本数下限未満で再計算された結果）
        public string HasiraSyu { get; set; }        //☆☆柱種類：柱Ａ、柱Ｂ
        public int SBSukima { get; set; }            //☆☆下端隙間（PS）。独立横格子でピッチ優先の場合、格子Hが短くなりその分隙間が増える場合がある
    }
    //格子ユニット情報出力ブロック
    public class aryUnit
    {
        public int No { get; set; }                  //番号
        public int Pitch1 { get; set; }              //ピッチ1
        public int Pitch1Su { get; set; }            //ピッチ1数
        public int Pitch2 { get; set; }              //ピッチ2
        public int Pitch2Su { get; set; }            //ピッチ2数
        public int Honsu { get; set; }               //格子本数
        public int Width { get; set; }               //幅※ユニット分割位置を表現するので隙間を含む、公差減算済み
        public int Kousa { get; set; }               //公差
        public string Sx { get; set; }               //横開始位置（公差込み。Scale考慮）
        public string Ex { get; set; }               //横終了位置（格子右面揃え。Scale考慮）
        public int Height { get; set; }              //高さ※ユニット分割位置を表現するので隙間を含む
        public string Sy { get; set; }               //縦開始位置（接続部中間位置なので格子長とは合わない。Scale考慮）
        public string Ey { get; set; }               //縦終了位置（接続部中間位置なので格子長とは合わない。Scale考慮）
        public int DoubutiSu { get; set; }           //ユニット内胴縁本数
        public int MX { get; set; }                  //胴縁長
        public int ZT { get; set; }                  //左はね出し(縦)/上はね出し(横)
        public int ZM { get; set; }                  //格子ピッチ1
        public int KA { get; set; }                  //格子本数1
        public int ZN { get; set; }                  //格子ピッチ2
        public int KB { get; set; }                  //格子本数2
        public int YT { get; set; }                  //右はね出し(縦)/下はね出し(横)
        public int MY { get; set; }                  //格子長
        public int ZS { get; set; }                  //下はね出し(縦)/左はね出し(横)
        public int ZF { get; set; }                  //胴縁ピッチ1（最下(縦)/最左(横)）
        public int ZG { get; set; }                  //胴縁ピッチ2（↓）
        public int ZI { get; set; }                  //胴縁ピッチ3（↓）
        public int ZL { get; set; }                  //胴縁ピッチ4（最上(縦)/最右(横)）
        public int YS { get; set; }                  //上はね出し(縦)/右はね出し(横)
        public int Wait { get; set; }                //重量(g)
    }
    //はりユニット情報（縦方向）出力ブロック
    public class aryHari
    {
        public int Top { get; set; }                 //はり位置（高さ芯（全体Hに対する位置））
        public int Top2 { get; set; }                //はり位置（高さ芯（対象胴縁取付材に対する位置。横格子時））
        public int Left { get; set; }                //はり位置（左面）
        public int Width { get; set; }               //はり長
        public int Height { get; set; }              //はり高さ（＝はりの見付）
        public int Pitch { get; set; }               //はりピッチ
        public int TargetNo { get; set; }            //対象のNo（縦格子の場合：縦方向ユニット分割、横格子の場合：胴縁取付材No（aryDTZaiのIndex）
    }
    //はりユニット情報（横方向）出力ブロック
    public class aryHariL
    {
        public int Width { get; set; }               //はり長
    }
    //胴縁取付材ユニット情報（横方向）出力ブロック
    public class aryDTZai
    {
        public int l { get; set; }                   //胴縁取付材長さ
        public int ZT { get; set; }                  //左はね出し（縦格子のみ）
        public int YT { get; set; }                  //右はね出し（縦格子のみ）
        public Boolean Kakou { get; set; }           //胴縁加工有効性。格子位置と干渉する場合False
    }
    //胴縁取付材ユニット情報（横方向）出力ブロック
    public class aryDTZaiL
    {
        public int Width { get; set; }               //胴縁取付材長さ
    }
    //柱ユニット情報出力ブロック
    public class aryHasira
    {
        public int No { get; set; }                  //番号
        public int YS { get; set; }                  //はりピッチ（一番上）
        public int ZF { get; set; }                  //はりピッチ１（1本目と2本目の間ピッチ
        public int ZG { get; set; }                  //はりピッチ２（2本目と3本目の間ピッチ※はりが3本の場合）
        public int ZS { get; set; }                  //はりの一番下から柱端部（飲み込み部の先端）※格子ZS＋下端隙間＋飲み込み350
        public int HL { get; set; }                  //柱長※入力の柱H＋350
        public int KZS { get; set; }                 //格子ユニットのZS
        public int SBSukima { get; set; }            //下端隙間（PS）※入力値のまま
        public int HPitch1 { get; set; }             //柱ピッチ1
        public int HPitch1Su { get; set; }           //柱ピッチ1数
        public int HPitch2 { get; set; }             //柱ピッチ2
        public int HPitch2Su { get; set; }           //柱ピッチ2数
    }
}
