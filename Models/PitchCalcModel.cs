using System;
using System.Collections.Generic;

namespace courtline.Models
{
    public class PitchCalcModel
    {
        public Result result { get; set; }
        public AryPitch aryPitch { get; set; }
        public PitchCalcModel()
        {
            this.result = new Result();
            this.aryPitch = new AryPitch();

        }
    }

    public class Result
    {
        //ステータスブロック
        public string status { get; set; }                 //"OK"、"NG"
        public string massage { get; set; }               //エラー内容メッセージ。statusが"OK"の場合は空文字。"NG"の場合、エラー理由を返却。
    }

    public class TypPitch
    {

        public long Pitch1 { get; set; }               //1種類目の格子ピッチ

        public long Pitch1Su { get; set; }             //1種類目の格子ピッチ数

        public long Pitch2 { get; set; }               //2種類目の格子ピッチ,二種類目が無い場合はゼロ

        public long Pitch2Su { get; set; }             //2種類目の格子ピッチ数,二種類目が無い場合はゼロ

        public long lngDefaultSa { get; set; }            // 

        public Boolean blnDefault { get; set; }           //

    }

    public class AryPitch
    {
        public int PitchCount { get; set; }               //ピッチリストの配列数
        public List<TypPitch> PitchList { get; set; }     //ピッチリスト(見出し)

        public AryPitch()
        {
            this.PitchList = new List<TypPitch>();
        }
    }


}

