using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Files;

namespace World.Map
{
    public class SetPiece
    {
        public static SetPiece Load(string name)
        {
            var map = MapElementFile.ReadFrom("Map/Files/SetPieces/" + name);
            return new SetPiece(map);
        }

        public MapElementFile file;

        public SetPiece(MapElementFile file)
        {
            this.file = file;
        }
    }
}
