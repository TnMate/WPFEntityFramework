using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tetris.Persistence;

namespace Tetris.Model
{

    public enum GameSize { Small, Medium, Large}

    public class TetrisModel
    {
        #region GameSize constans

        private const Int16 GameSizeSmall = 4;
        private const Int16 GameSizeMedium = 8;
        private const Int16 GameSizeLarge = 12;

        #endregion

        #region Fields

        private TetrisDataAccessInterface _dataAccess;
        private TetrisTable _table;
        private GameSize _gameSize;
        private Boolean _isLost;

        #endregion

        #region Events

        public event EventHandler<TetrisEventArgs> GameAdvanced;

        public event EventHandler<TetrisEventArgs> GameOver;

        #endregion

        #region Constructors

        public TetrisModel(TetrisDataAccessInterface dataAccess = null)
        {
            _gameSize = GameSize.Medium;
            _table = new TetrisTable();
            NewShapeSpawn();
            _dataAccess = dataAccess;
        }

        #endregion

        #region Public methods

        public void NewGame()
        {
            _isLost = false;
            switch (_gameSize)
            {
                case GameSize.Small:
                    _table = new TetrisTable(GameSizeSmall);
                    NewShapeSpawn();
                    break;
                case GameSize.Medium:
                    _table = new TetrisTable(GameSizeMedium);
                    NewShapeSpawn();
                    break;
                case GameSize.Large:
                    _table = new TetrisTable(GameSizeLarge);
                    NewShapeSpawn();
                    break;
                default:
                    _table = new TetrisTable();
                    NewShapeSpawn();
                    break;
            }
        }

        public void AdvanceTime()
        {
            if(!TimeAdvanced())
            {
                _isLost = true;
                gameOver();
            }
            gameAdvanced();
        }

        public void SetTableSize(Int32 size)
        {
            switch (size)
            {
                case 0:
                    _gameSize = GameSize.Small;
                    break;
                case 1:
                    _gameSize = GameSize.Medium;
                    break;
                case 2:
                    _gameSize = GameSize.Large;
                    break;
                default:
                    _gameSize = GameSize.Medium;
                    break;
            }
        }

        public Int32 getTableSize()
        {
            switch (_gameSize)
            {
                case GameSize.Small:
                    return GameSizeSmall;
                case GameSize.Medium:
                    return GameSizeMedium;
                case GameSize.Large:
                    return GameSizeLarge;
                default:
                    return GameSizeMedium;
            }
        }

        public Int32 getTime()
        {
            return _table.Time;
        }
        public Int32[,] getTable()
        {
            return _table._typeTable;
        }

        public void GoLeft()
        {
            if (CheckAndMoveLeftPosition())
                gameAdvanced();
        }
        
        public void GoRight()
        {
            if (CheckAndMoveRightPosition())
                gameAdvanced();
        }

        public void GoDown()
        {
            if (!_isLost)
            {
                if (!CheckAndMoveDownPosition())
                {
                    DeleteCompletedRows();
                    NewShapeSpawn();
                }
                gameAdvanced();
            }
        }

        public void RotateMeSenpai()
        {
            if(CheckAndRotatePosition())
                gameAdvanced();
        }

        public TetrisTable table
        {
            get { return _table; }
        }

        public GameSize GameSize
        {
            get { return _gameSize; } set { _gameSize = value; }
        }
        
        public Boolean isLost
        {
            get { return _isLost; }
        }

        public async Task LoadGameAsync(String path)
        {
            if (_dataAccess == null)
                throw new InvalidOperationException("No data access is provided.");

            _isLost = false;
            _table = await _dataAccess.LoadAsync(path);
            

            switch (_table.Size)
            {
                case 4:
                    _gameSize = GameSize.Small;
                    break;
                case 8:
                    _gameSize = GameSize.Medium;
                    break;
                case 12:
                    _gameSize = GameSize.Large;
                    break;
            }
        }

        public async Task SaveGameAsync(String path)
        {
            if (_dataAccess == null)
                throw new InvalidOperationException("No data access is provided.");

            if (_isLost)
                throw new Exception("You can't save a lost game!");

            await _dataAccess.SaveAsync(path, _table);
        }

        public async Task<ICollection<SaveEntry>> ListGamesAsync()
        {

            if (_dataAccess == null)
                throw new InvalidOperationException("No data access is provided.");

            return await _dataAccess.ListAsync();
        }
        #endregion

        #region private methods

        private void gameAdvanced()
        {
            if (GameAdvanced != null)
            {
                GameAdvanced(this, new TetrisEventArgs(_table ,_table.Time,_isLost));
            }
        }

        private void gameOver()
        {
            if (GameOver != null)
            {
                GameOver(this, new TetrisEventArgs(_table , _table.Time, _isLost));
            }
        }

        #endregion

        #region TetrisTableMethods

        static ShapeTypes RandomEnumValue()
        {
            var v = Enum.GetValues(typeof(ShapeTypes));
            return (ShapeTypes)v.GetValue(new Random().Next(v.Length));
        }

        public Boolean NewShapeSpawn()
        {
            Shape currentShape;
            Int32 currentShapeIndex;
            ShapeTypes value = RandomEnumValue();
            switch (value)
            {
                case ShapeTypes.TShape:
                    currentShape = new TShape();
                    currentShapeIndex = 1;
                    break;
                case ShapeTypes.JShape:
                    currentShape = new JShape();
                    currentShapeIndex = 2;
                    break;
                case ShapeTypes.ZShape:
                    currentShape = new ZShape();
                    currentShapeIndex = 3;
                    break;
                case ShapeTypes.OShape:
                    currentShape = new OShape();
                    currentShapeIndex = 4;
                    break;
                case ShapeTypes.SShape:
                    currentShape = new SShape();
                    currentShapeIndex = 5;
                    break;
                case ShapeTypes.LShape:
                    currentShape = new LShape();
                    currentShapeIndex = 6;
                    break;
                case ShapeTypes.IShape:
                    currentShape = new IShape();
                    currentShapeIndex = 7;
                    break;
                default:
                    currentShape = new IShape();
                    currentShapeIndex = 7;
                    break;
            }

            var x = currentShape.getPosX();
            var y = currentShape.getPosY();
            var ImaginaryPos = currentShape.getCurrentState();

            Boolean l = true;

            for (int i = 0; i < 4 && l; i++)
            {
                var a = x + ImaginaryPos[i, 0];
                var b = y + ImaginaryPos[i, 1];

                if (a < 0 || a > _table.Size || b < 0 || b > 15)
                {
                    l = false;
                }

                l = l && _table.Table[a, b] == 0;
            }


            if (l)
            {

                for (int i = 0; i < 4 && l; i++)
                {
                    var a = x + ImaginaryPos[i, 0];
                    var b = y + ImaginaryPos[i, 1];

                    _table.Table[a, b] = 2;
                    _table.TypeTable[a, b] = currentShapeIndex;
                }
                _table.NewShape(currentShapeIndex);
            }
            else
            {
                currentShape = null;
                //gameover();
            }
            return l;

        }
        public void DeleteCompletedRows()
        {
            for (int i = 0; i <= _table.Size; i++)
            {
                Boolean l = true;
                for (int j = 0; j < 16 && l; j++)
                {
                    l = _table.Table[i, j] == 1;
                }
                if (l)
                {
                    for (int j = i; j > 0; j--)
                    {
                        for (int k = 0; k < 16; k++)
                        {
                            _table.Table[j, k] = _table.Table[j - 1, k];
                            _table.TypeTable[j, k] = _table.TypeTable[j - 1, k];
                        }
                    }
                }
            }
        }
        public Boolean CheckAndMoveLeftPosition()
        {
            if (_table.CurrentShape != null)
            {
                var x = _table.CurrentShape.getPosX();
                var y = _table.CurrentShape.getPosY();
                var blocks = _table.CurrentShape.getCurrentState();

                y--;

                Boolean l = true;

                for (int i = 0; i < 4 && l; i++)
                {
                    var a = x + blocks[i, 0];
                    var b = y + blocks[i, 1];

                    if (a < 0 || a > _table.Size || b < 0 || b > 15)
                    {
                        l = false;
                    }

                    l = l && (_table.Table[a, b] == 0 || _table.Table[a, b] == 2);
                }

                if (l)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        var a = _table.CurrentShape.getPosX() + blocks[i, 0];
                        var b = _table.CurrentShape.getPosY() + blocks[i, 1];

                        _table.Table[a, b] = 0;
                        _table.TypeTable[a, b] = 0;
                    }
                    _table.CurrentShape.moveToLeft();
                    for (int i = 0; i < 4; i++)
                    {
                        var a = _table.CurrentShape.getPosX() + blocks[i, 0];
                        var b = _table.CurrentShape.getPosY() + blocks[i, 1];

                        _table.Table[a, b] = 2;
                        _table.TypeTable[a, b] = _table.CurrentShapeIndex;
                    }
                }
                return l;
            }
            return false;
        }
        public Boolean CheckAndMoveDownPosition()
        {
            if (_table.CurrentShape != null)
            {
                var x = _table.CurrentShape.getPosX();
                var y = _table.CurrentShape.getPosY();
                var blocks = _table.CurrentShape.getCurrentState();

                x++;

                Boolean l = true;

                for (int i = 0; i < 4 && l; i++)
                {
                    var a = x + blocks[i, 0];
                    var b = y + blocks[i, 1];

                    if (a < 0 || a > _table.Size || b < 0 || b > 15)
                    {
                        l = false;
                    }

                    l = l && (_table.Table[a, b] == 0 || _table.Table[a, b] == 2);
                }

                if (l)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        var a = _table.CurrentShape.getPosX() + blocks[i, 0];
                        var b = _table.CurrentShape.getPosY() + blocks[i, 1];

                        _table.Table[a, b] = 0;
                        _table.TypeTable[a, b] = 0;
                    }
                    _table.CurrentShape.moveDown();
                    for (int i = 0; i < 4; i++)
                    {
                        var a = _table.CurrentShape.getPosX() + blocks[i, 0];
                        var b = _table.CurrentShape.getPosY() + blocks[i, 1];

                        _table.Table[a, b] = 2;
                        _table.TypeTable[a, b] = _table.CurrentShapeIndex;
                    }
                }
                else
                {
                    for (int i = 0; i < 4; i++)
                    {
                        var a = _table.CurrentShape.getPosX() + blocks[i, 0];
                        var b = _table.CurrentShape.getPosY() + blocks[i, 1];

                        _table.Table[a, b] = 1;
                    }
                }

                return l;
            }
            return false;

        }
        public Boolean CheckAndMoveRightPosition()
        {
            if (_table.CurrentShape != null)
            {
                var x = _table.CurrentShape.getPosX();
                var y = _table.CurrentShape.getPosY();
                var blocks = _table.CurrentShape.getCurrentState();

                y++;

                Boolean l = true;

                for (int i = 0; i < 4 && l; i++)
                {
                    var a = x + blocks[i, 0];
                    var b = y + blocks[i, 1];

                    if (a < 0 || a > _table.Size || b < 0 || b > 15)
                    {
                        l = false;
                    }

                    l = l && (_table.Table[a, b] == 0 || _table.Table[a, b] == 2);
                }

                if (l)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        var a = _table.CurrentShape.getPosX() + blocks[i, 0];
                        var b = _table.CurrentShape.getPosY() + blocks[i, 1];

                        _table.Table[a, b] = 0;
                        _table.TypeTable[a, b] = 0;
                    }
                    _table.CurrentShape.moveToRight();
                    for (int i = 0; i < 4; i++)
                    {
                        var a = _table.CurrentShape.getPosX() + blocks[i, 0];
                        var b = _table.CurrentShape.getPosY() + blocks[i, 1];

                        _table.Table[a, b] = 2;
                        _table.TypeTable[a, b] = _table.CurrentShapeIndex;
                    }
                }
                return l;
            }
            return false;
        }
        public Boolean CheckAndRotatePosition()
        {
            if (_table.CurrentShape != null)
            {
                var x = _table.CurrentShape.getPosX();
                var y = _table.CurrentShape.getPosY();
                var blocks = _table.CurrentShape.getNextRotateState();

                Boolean l = true;

                for (int i = 0; i < 4 && l; i++)
                {
                    var a = x + blocks[i, 0];
                    var b = y + blocks[i, 1];

                    if (a < 0 || a > _table.Size || b < 0 || b > 15)
                    {
                        l = false;
                    }

                    l = l && (_table.Table[a, b] == 0 || _table.Table[a, b] == 2);
                }

                if (l)
                {
                    var realShape = _table.CurrentShape.getCurrentState();
                    for (int i = 0; i < 4; i++)
                    {
                        var a = x + realShape[i, 0];
                        var b = y + realShape[i, 1];

                        _table.Table[a, b] = 0;
                        _table.TypeTable[a, b] = 0;
                    }
                    _table.CurrentShape.rotateShape();
                    for (int i = 0; i < 4; i++)
                    {
                        var a = _table.CurrentShape.getPosX() + blocks[i, 0];
                        var b = _table.CurrentShape.getPosY() + blocks[i, 1];

                        _table.Table[a, b] = 2;
                        _table.TypeTable[a, b] = _table.CurrentShapeIndex;
                    }
                }
                return l;
            }
            return false;
        }
        public Boolean TimeAdvanced()
        {
            _table.Time = _table.Time + 1;
            if (!CheckAndMoveDownPosition())
            {
                DeleteCompletedRows();
                if (!NewShapeSpawn())
                {
                    return false;
                }
                return true;
            }
            return true;
        }

        #endregion
    }
}
