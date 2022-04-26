using System;
namespace courtline
{
    public class InputJSON
    {
        public string Osamari { get; set; }         //壁内、持出し、独立
        public string TateYoko { get; set; }        //縦格子、横格子
        public int l{ get; set; }                   //L寸法
        public int H{ get; set; }                   //H寸法（or柱H寸法）
        public int a{ get; set; }                   //a寸法（壁～格子隙間（左））※独立時は押えが異なる
        public int b{ get; set; }                   //b寸法（格子～壁隙間（右））※独立時は押えが異なる
        public int c{ get; set; }                   //c寸法（壁～胴縁・はり隙間（左））※独立時は押えが異なる
        public int d{ get; set; }                   //d寸法（胴縁・はり～壁隙間（右））※独立時は押えが異なる
        public int MaxW{ get; set; }                //希望分割寸法Ｗ
        public int MaxH{ get; set; }                //希望分割寸法Ｈ
        public string Kousi { get; set; }           //格子種類
        public int Kpitch{ get; set; }              //格子ピッチ
        public string KPitchText { get; set; }      //格子ピッチテキスト（これがある場合はピッチ選択画面からの再表示と見なす）
        public int Mituke{ get; set; }              //格子見付
        public int Mikomi{ get; set; }              //格子見込
        public int TWait{ get; set; }               //単位重量（g）
        public string YMode { get; set; }           //優先モード（出来寸優先、ピッチ優先）
        public string Renketu { get; set; }         //格子連結方法（キャップ、スリーブ）
        public int TSukima{ get; set; }             //縦格子格子間隙間
        public int YSukima{ get; set; }             //横格子格子間隙間
        public string HMode { get; set; }           //はり位置（はり前連結、はり間連結）
        public int HPitch{ get; set; }              //☆☆柱ピッチ
        public int SBSukima{ get; set; }            //☆☆下端隙間
        public int DTZaiMaxH{ get; set; }           //☆☆希望胴縁取付材分割寸法Ｈ（横格子、はり間連結の時のみ使用する）
        public string LDeiri { get; set; }          //◎◎左端部出入隅（端部/出隅/入隅/空文字）※これを意識するのは独立横格子の端部胴縁位置
        public string RDeiri { get; set; }          //◎◎右端部出入隅（端部/出隅/入隅/空文字）※これを意識するのは独立横格子の端部胴縁位置
        public string LKatiMake { get; set; }       //◎◎左端部勝ち負け（勝/負/空文字）       ※これを意識するのは独立横格子の端部胴縁位置
        public string RKatiMake { get; set; }       //◎◎右端部勝ち負け（勝/負/空文字）       ※これを意識するのは独立横格子の端部胴縁位置
        public string KCorner { get; set; }         //◎◎コーナー格子使用（なし/あり）        ※これを意識するのは独立縦格子
    }
}
