using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Win2D.UWP
{
    public class DataBaseHelper
    {
        const string sp = "\n\n";

        public static readonly string[] _inOtherWords = new[]
        {
            $"Hebrews 1:2-3{sp}1 God, who at sundry times and in divers manners spake in time past unto the fathers by the prophets, 2 Hath in these last days spoken unto us by his Son, whom he hath appointed heir of all things, by whom also he made the worlds;{sp}King James Version (KJV)",
            $"Proverbs 3:5{sp}5 Trust in the LORD with all your heart; and lean not to your own understanding. 6 In all your ways acknowledge him, and he shall direct your paths.{sp}King James Version (KJV)",

        };

        public static readonly string[] _textsOfMencius = new[]
        {
            $"希伯來書 1:1-2{sp}1 過去，神在不同的時代，用不同的方式，藉著先知們對祖先說話， 2 但在末後的這些日子裡[a]，神藉著他的[b]兒子向我們說話。神預定了他為萬有的繼承人，也藉著他造了宇宙[c]。{sp}Chinese Standard Bible (Traditional)",
            $"Proverbs 3:5{sp}你要全心信靠耶和华，\r\n    不可倚靠自己的悟性。\r\n6 凡事都要寻求祂，\r\n    祂必指引你走正路。{sp}Chinese Contemporary Bible (Simplified)",
        };

        public static readonly string[] _textsOfMakenaide = new[]
        {
            "ふとした瞬間に 視線がぶつかる",
            "幸福のときめき 覚えているでしょ",
            "パステルカラーの季節に恋した",
            "あの日のように 輝いてる",
            "あなたでいてね",
            "負けないで もう少し",
            "最後まで 走り抜けて",
            "どんなに 離れてても",
            "心は そばにいるわ",
            "追いかけて 遥かな夢を",
        };

        public static readonly string[] _textsOfOdeToJoy = new[]
        {
            "Wem der große Wurf gelungen,",
            "Eines Freundes Freund zu sein;",
            "Wer ein holdes Weib errungen,",
            "Mische seinen Jubel ein!",
            "Ja, wer auch nur eine Seele",
            "Sein nennt auf dem Erdenrund!",
            "Und wer's nie gekonnt, der stehle",
            "Weinend sich aus diesem Bund!",
        };
    }
}
