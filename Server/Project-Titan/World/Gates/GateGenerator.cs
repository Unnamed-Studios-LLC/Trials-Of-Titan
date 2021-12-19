using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TitanCore.Data;
using TitanCore.Data.Map;
using TitanCore.Files;
using Utils.NET.Geometry;
using World.Map;
using static TitanCore.Files.MapElementFile;

namespace World.Gates
{
    public class GateGenerator
    {
        private enum ElementTags : byte
        {
            Empty = 0,
            Ground = 1,
            Wall = 2,
            SetPiece = 4,

            SetPieceGround = Ground | SetPiece,
            SetPieceWall = Wall | SetPiece,
        }

        private struct DrawnSetPiece
        {
            public Int2 position;

            public SetPieceDrawType drawType;

            public SetPiece setPiece;
        }

        protected enum SetPieceDrawType
        {
            Exact,
            CutInto
        }

        protected virtual int CanvasSize => 600;

        protected virtual string[] SetPieces { get; }

        private Int2 SpawnPosition => new Int2(CanvasSize / 2, CanvasSize / 2);

        private Dictionary<string, SetPiece> setPieces;

        private List<DrawnSetPiece> drawnSetPieces = new List<DrawnSetPiece>();

        private ElementTags[,] canvas;

        private Int2 brushPosition;

        private IntRect bounds;

        public GateGenerator()
        {

        }

        public MapElementFile LoadMap()
        {
            Init();

            Layout();

            return Rasterize();
        }

        #region Init

        private void Init()
        {
            InitSetPieces();
            InitCanvas();
        }

        private void InitSetPieces()
        {
            setPieces = SetPieces.ToDictionary(_ => _, _ => SetPiece.Load(_));
        }

        private void InitCanvas()
        {
            canvas = new ElementTags[CanvasSize, CanvasSize];
            bounds = new IntRect(SpawnPosition.x, SpawnPosition.y, 0, 0);
        }

        private void InitBrush()
        {
            brushPosition = SpawnPosition;
        }

        #endregion

        #region Layout

        private void Layout()
        {
            LayoutObstructions();
        }

        protected virtual void LayoutSpawn()
        {

        }

        protected virtual void LayoutBoss()
        {

        }

        protected virtual void LayoutObstructions()
        {

        }


        #endregion

        #region Rasterize

        private MapElementFile Rasterize()
        {
            return null;
        }

        #endregion

        #region Canvas

        protected void DrawSpawn(string setPieceName, SetPieceDrawType drawType)
        {
            var setPiece = setPieces[setPieceName];
            DrawSetPiece(new Int2(0, 0), setPiece, drawType, false);
        }

        protected void DrawBoss(Int2 relativePosition, string setPieceName, SetPieceDrawType drawType)
        {
            var setPiece = setPieces[setPieceName];
            DrawSetPiece(relativePosition, setPiece, drawType, false);
        }

        protected void DrawSetPiece(Int2 relativePosition, string setPieceName, SetPieceDrawType drawType)
        {
            DrawSetPiece(relativePosition, setPieces[setPieceName], drawType, false);
        }

        private void DrawSetPiece(Int2 relativePosition, SetPiece setPiece, SetPieceDrawType drawType, bool moveBrush)
        {
            var position = brushPosition + relativePosition;

            for (int y = 0; y < setPiece.file.height; y++)
                for (int x = 0; x < setPiece.file.width; x++)
                {
                    var point = position + new Int2(x, y);

                    var element = setPiece.file.tiles[x, y];
                    var elementType = GetTags(element);

                    if (drawType == SetPieceDrawType.Exact)
                        Draw(point, elementType);
                    else
                    {
                        var current = Get(point);
                        if (HasTag(current, ElementTags.Ground))
                        {
                            elementType = RemoveTag(elementType, ElementTags.Wall);
                        }

                        Draw(point, elementType);
                    }
                }

            drawnSetPieces.Add(new DrawnSetPiece
            {
                position = position,
                drawType = drawType,
                setPiece = setPiece
            });

            if (moveBrush)
                brushPosition = position;
        }

        private ElementTags GetTags(MapTileElement tileElement)
        {
            var tags = ElementTags.SetPiece;
            if (tileElement.objectType > 0)
            {
                var info = GameData.objects[tileElement.objectType];
                if (info is WallInfo)
                {
                    tags |= ElementTags.Wall;
                }
            }

            if (tileElement.tileType > 0)
            {
                tags |= ElementTags.Ground;
            }

            return tags;
        }

        private void Draw(Int2 position, ElementTags type)
        {
            canvas[position.x, position.y] = type;
        }

        private ElementTags Get(Int2 position)
        {
            return canvas[position.x, position.y];
        }

        private bool HasTag(ElementTags tags, ElementTags tag)
        {
            return (tags & tag) == tag;
        }

        private ElementTags RemoveTag(ElementTags tags, ElementTags tag)
        {
            return tags & ~tag;
        }

        #endregion
    }
}
