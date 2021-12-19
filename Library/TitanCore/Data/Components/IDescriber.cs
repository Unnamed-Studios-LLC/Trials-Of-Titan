using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;

namespace TitanCore.Data.Components
{
    public interface IDescriber
    {
        GameColor GetNeutralColor();

        GameColor GetEnchantColor(int enchantLevel);

        void NewLine();

        void AddContent(string content);

        void AddContent(string content, GameColor color);

        void AddTitle(string name);

        void AddTitle(string name, GameColor color);

        void AddElement(string element);

        void AddElement(string element, GameColor color);

        void AddLineSeparator();

        void Clear();

        bool GetAllowsEmptyTitle();

        bool GetAllowsSprites();

        string GetClickSprite();
    }
}
