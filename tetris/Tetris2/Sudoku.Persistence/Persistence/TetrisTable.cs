using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tetris.Persistence
{

    public enum ShapeTypes { TShape,JShape,ZShape,OShape,SShape,LShape,IShape}

    public class TetrisTable
    {
        static ShapeTypes RandomEnumValue()
        {
            var v = Enum.GetValues(typeof(ShapeTypes));
            return (ShapeTypes)v.GetValue(new Random().Next(v.Length));
        }

        #region Fields

        public Byte[,] _tetrisTable;
        public Int32[,] _typeTable;
        private Shape currentShape;
        private Int32 currentShapeIndex;
        private Int32 mapSize;
        private Int32 gameTime;

        #endregion

        #region Constructors

        public TetrisTable(Int32 size = 8)
        {
            _tetrisTable = new byte[size + 1, 16];
            _typeTable = new Int32[size + 1, 16];
            mapSize = size;
            gameTime = 0;
        }
        public TetrisTable(Int32 size, Int32 time, Int32 shape, Int32 cordx, Int32 cordy, Int32 state, byte[,] table, Int32[,] colorTable)
        {
            mapSize = size;
            gameTime = time;
            currentShapeIndex = shape;
            switch (shape)
            {
                case 1:
                    currentShape = new TShape(cordx, cordy, state);
                    currentShapeIndex = 1;
                    break;
                case 2:
                    currentShape = new JShape(cordx, cordy, state);
                    currentShapeIndex = 2;
                    break;
                case 3:
                    currentShape = new ZShape(cordx, cordy, state);
                    currentShapeIndex = 3;
                    break;
                case 4:
                    currentShape = new OShape(cordx, cordy, state);
                    currentShapeIndex = 4;
                    break;
                case 5:
                    currentShape = new SShape(cordx, cordy, state);
                    currentShapeIndex = 5;
                    break;
                case 6:
                    currentShape = new LShape(cordx, cordy, state);
                    currentShapeIndex = 6;
                    break;
                case 7:
                    currentShape = new IShape(cordx, cordy, state);
                    currentShapeIndex = 7;
                    break;
                default:
                    break;
            }
            _tetrisTable = table;
            _typeTable = colorTable;
        }

        #endregion

        #region public methods

        public void SetValue(Int32 x, Int32 y, Byte value, Int32 type)
        {
            _tetrisTable[x, y] = value;
            _typeTable[x, y] = type;
        }

        public void NewShape(Int32 index)
        {
            switch (index)
            {
                case 1:
                    currentShape = new TShape();
                    currentShapeIndex = 1;
                    break;
                case 2:
                    currentShape = new JShape();
                    currentShapeIndex = 2;
                    break;
                case 3:
                    currentShape = new ZShape();
                    currentShapeIndex = 3;
                    break;
                case 4:
                    currentShape = new OShape();
                    currentShapeIndex = 4;
                    break;
                case 5:
                    currentShape = new SShape();
                    currentShapeIndex = 5;
                    break;
                case 6:
                    currentShape = new LShape();
                    currentShapeIndex = 6;
                    break;
                case 7:
                    currentShape = new IShape();
                    currentShapeIndex = 7;
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region Get methods

        public Int32 Time
        {
            get { return gameTime; }
            set { gameTime = value; }
        }
        public Int32 Size
        {
            get { return mapSize; }
        }
        public Int32 CurrentShapeIndex
        {
            get { return currentShapeIndex; }
        }
        public Int32 ShapeCordX
        {
            get { return currentShape.getPosX(); }
        }
        public Int32 ShapeCordY
        {
            get { return currentShape.getPosY(); }
        }
        public Byte[,] Table
        {
            get { return _tetrisTable; }
        }
        public Int32[,] TypeTable
        {
            get { return _typeTable; }
        }
        public Int32 ShapeRotation
        {
            get { return currentShape.getIntState(); }
        }
        public Shape CurrentShape
        {
            get { return currentShape; }
        }

        #endregion

    }

}
