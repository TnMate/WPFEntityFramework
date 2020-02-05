using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Tetris.Model;
using Tetris.Persistence;

namespace Tetris2.ViewModel
{
    class GameViewModel : ViewModelBase
    {

        #region Fields

        private TetrisModel _model;
        private SaveEntry _selectedGame;
        private String _newName = String.Empty;
        private Int32 RowCount;
        private Int32 Heighte;


        #endregion

        #region Properties

        public DelegateCommand LoadGameOpenCommand { get; private set; }

        public DelegateCommand LoadGameCloseCommand { get; private set; }

        public DelegateCommand SaveGameOpenCommand { get; private set; }

        public DelegateCommand SaveGameCloseCommand { get; private set; }

        public DelegateCommand NewGameCommand { get; private set; }

        public DelegateCommand LoadGameCommand { get; private set; }

        public DelegateCommand SaveGameCommand { get; private set; }

        public DelegateCommand SmallGameCommand { get; private set; }

        public DelegateCommand MediumGameCommand { get; private set; }

        public DelegateCommand LargeGameCommand { get; private set; }

        public DelegateCommand ExitCommand { get; private set; }

        public DelegateCommand PauseCommand { get; private set; }

        public ObservableCollection<GameField> Fields { get; set; }

        public Int32 GameTableSize { get { return _model.table.Size; } }

        public String GameTime { get { return TimeSpan.FromSeconds(_model.table.Time).ToString("g"); } }

        public Boolean IsGameSmall
        {
            get { return _model.GameSize == GameSize.Small; }
            set
            {
                if (_model.GameSize == GameSize.Small)
                    return;

                _model.GameSize = GameSize.Small;
                OnPropertyChanged("IsGameSmall");
                OnPropertyChanged("IsGameMedium");
                OnPropertyChanged("IsGameLarge");
            }
        }

        public Boolean IsGameMedium
        {
            get { return _model.GameSize == GameSize.Medium; }
            set
            {
                if (_model.GameSize == GameSize.Medium)
                    return;

                _model.GameSize = GameSize.Medium;
                OnPropertyChanged("IsGameSmall");
                OnPropertyChanged("IsGameMedium");
                OnPropertyChanged("IsGameLarge");
            }
        }

        public Boolean IsGameLarge
        {
            get { return _model.GameSize == GameSize.Large; }
            set
            {
                if (_model.GameSize == GameSize.Large)
                    return;

                _model.GameSize = GameSize.Large;
                OnPropertyChanged("IsGameSmall");
                OnPropertyChanged("IsGameMedium");
                OnPropertyChanged("IsGameLarge");
            }
        }

        public DelegateCommand MoveRightCommand { get; private set; }

        public DelegateCommand MoveLeftCommand { get; private set; }

        public DelegateCommand MoveUpCommand { get; private set; }

        public DelegateCommand MoveDownCommand { get; private set; }

        public Int32 Rows
        {
            get => RowCount;
            set
            {
                if (RowCount != value)
                {
                    RowCount = value;
                    OnPropertyChanged();
                }
            }
        }

        public Int32 WindowHeight
        {
            get => Heighte;
            set
            {
                if (Heighte != value)
                {
                    Heighte = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region Events

        /// <summary>
        /// Új játék eseménye.
        /// </summary>
        public event EventHandler NewGame;

        /// <summary>
        /// Játék betöltésének eseménye.
        /// </summary>
        public event EventHandler LoadGame;

        /// <summary>
        /// Játék mentésének eseménye.
        /// </summary>
        public event EventHandler SaveGame;

        /// <summary>
        /// Játékból való kilépés eseménye.
        /// </summary>
        public event EventHandler ExitGame;

        /// <summary>
        /// Játék szüneteltetésének eseménye.
        /// </summary>
        public event EventHandler PauseGame;

        public event EventHandler SmallGame;

        public event EventHandler MediumGame;

        public event EventHandler LargeGame;

        public event EventHandler Left;

        public event EventHandler Right;

        public event EventHandler Up;

        public event EventHandler Down;

        /// <summary>
        /// Játék betöltés választásának eseménye.
        /// </summary>
        public event EventHandler LoadGameOpen;

        /// <summary>
        /// Játék betöltésének eseménye.
        /// </summary>
        public event EventHandler<String> LoadGameClose;

        /// <summary>
        /// Játék mentés választásának eseménye.
        /// </summary>
        public event EventHandler SaveGameOpen;

        /// <summary>
        /// Játék mentésének eseménye.
        /// </summary>
        public event EventHandler<String> SaveGameClose;

        /// <summary>
        /// Perzisztens játékállapot mentések lekérdezése.
        /// </summary>
        public ObservableCollection<SaveEntry> Games { get; set; }

        /// <summary>
        /// Kiválasztott játékállapot mentés lekérdezése.
        /// </summary>
        public SaveEntry SelectedGame
        {
            get { return _selectedGame; }
            set
            {
                _selectedGame = value;
                if (_selectedGame != null)
                    NewName = String.Copy(_selectedGame.Name);

                OnPropertyChanged();
                LoadGameCloseCommand.RaiseCanExecuteChanged();
                SaveGameCloseCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Új játék mentés nevének lekérdezése.
        /// </summary>
        public String NewName
        {
            get { return _newName; }
            set
            {
                _newName = value;

                OnPropertyChanged();
                SaveGameCloseCommand.RaiseCanExecuteChanged();
            }
        }

        #endregion

        #region Constructors

        public GameViewModel(TetrisModel model)
        {
            // játék csatlakoztatása
            _model = model;
            _model.GameAdvanced += new EventHandler<TetrisEventArgs>(model_gameAdvanced);
            _model.GameOver += new EventHandler<TetrisEventArgs>(model_gameOver);

            // parancsok kezelése
            NewGameCommand = new DelegateCommand(param => OnNewGame());
            LoadGameCommand = new DelegateCommand(param => OnLoadGame());
            SaveGameCommand = new DelegateCommand(param => OnSaveGame());
            PauseCommand = new DelegateCommand(param => OnPauseGame());
            ExitCommand = new DelegateCommand(param => OnExitGame());
            SmallGameCommand = new DelegateCommand(param => OnSmallGame());
            MediumGameCommand = new DelegateCommand(param => OnMediumGame());
            LargeGameCommand = new DelegateCommand(param => OnLargeGame());
            MoveRightCommand = new DelegateCommand(param => RightPressed());
            MoveLeftCommand = new DelegateCommand(param => LeftPressed());
            MoveUpCommand = new DelegateCommand(param => UpPressed());
            MoveDownCommand = new DelegateCommand(param => DownPressed());

            LoadGameOpenCommand = new DelegateCommand(async param =>
            {
                Games = new ObservableCollection<SaveEntry>(await _model.ListGamesAsync());
                OnLoadGameOpen();
            });
            LoadGameCloseCommand = new DelegateCommand(
                param => SelectedGame != null, // parancs végrehajthatóságának feltétele
                param => { OnLoadGameClose(SelectedGame.Name); });
            SaveGameOpenCommand = new DelegateCommand(async param =>
            {
                Games = new ObservableCollection<SaveEntry>(await _model.ListGamesAsync());
                OnSaveGameOpen();
            });
            SaveGameCloseCommand = new DelegateCommand(
                param => NewName.Length > 0, // parancs végrehajthatóságának feltétele
                param => { OnSaveGameClose(NewName); });

            // játéktábla létrehozása
            Fields = new ObservableCollection<GameField>();
            GenerateFields();
            RefreshTable();

            this.WindowHeight = 400;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Mezők generálása.
        /// </summary>
        private void GenerateFields()
        {
            Fields.Clear();
            for (Int32 i = 0; i < _model.table.Size; i++) // inicializáljuk a mezőket
            {
                for (Int32 j = 0; j < 16; j++)
                {
                    Fields.Add(new GameField
                    {
                        IsLocked = false,
                        Type = 0,
                        X = i,
                        Y = j,
                        Number = i * _model.table.Size + j,
                        //StepCommand = new DelegateCommand(param => StepGame(Convert.ToInt32(param)))
                    });
                }
            }
        }

        /// <summary>
        /// Tábla frissítése.
        /// </summary>
        private void RefreshTable()
        {
            foreach (GameField field in Fields) // inicializálni kell a mezőket is
            {
                field.Type = _model.table.TypeTable[field.X+1,field.Y];
                //field.IsLocked = _model.table.IsLocked(field.X, field.Y);
            }

            OnPropertyChanged("GameTime");
        }

        #endregion

        #region Game event handlers

        /// <summary>
        /// Játék végének eseménykezelője.
        /// </summary>
        private void model_gameOver(object sender, TetrisEventArgs e)
        {
            foreach (GameField field in Fields)
            {
                field.IsLocked = true;
            }
        }

        /// <summary>
        /// Játék előrehaladásának eseménykezelője.
        /// </summary>
        private void model_gameAdvanced(object sender, TetrisEventArgs e)
        {
            OnPropertyChanged("GameTime");
            RefreshTable();
        }

        #endregion

        #region Event methods

        /// <summary>
        /// Játék betöltés választásának eseménykiváltása.
        /// </summary>
        private void OnLoadGameOpen()
        {
            if (LoadGameOpen != null)
                LoadGameOpen(this, EventArgs.Empty);
            GenerateFields();
            RefreshTable();
        }

        /// <summary>
        /// Játék betöltésének eseménykiváltása.
        /// </summary>
        private void OnLoadGameClose(String name)
        {
            if (LoadGameClose != null)
                LoadGameClose(this, name);
        }

        /// <summary>
        /// Játék mentés választásának eseménykiváltása.
        /// </summary>
        private void OnSaveGameOpen()
        {
            if (SaveGameOpen != null)
                SaveGameOpen(this, EventArgs.Empty);
        }

        /// <summary>
        /// Játék mentésének eseménykiváltása.
        /// </summary>
        private void OnSaveGameClose(String name)
        {
            if (SaveGameClose != null)
                SaveGameClose(this, name);
        }

        private void OnSmallGame()
        {
            if (SmallGame != null)
                SmallGame(this, EventArgs.Empty);

            GenerateFields();
            RefreshTable();
        }

        private void OnMediumGame()
        {
            if (MediumGame != null)
                MediumGame(this, EventArgs.Empty);
            GenerateFields();
            RefreshTable();
        }

        private void OnLargeGame()
        {

            if (LargeGame != null)
                LargeGame(this, EventArgs.Empty);
            GenerateFields();
            RefreshTable();
        }

        /// <summary>
        /// Új játék indításának eseménykiváltása.
        /// </summary>
        private void OnNewGame()
        {

            if (NewGame != null)
                NewGame(this, EventArgs.Empty);
            GenerateFields();
            RefreshTable();
        }


        /// <summary>
        /// Játék betöltése eseménykiváltása.
        /// </summary>
        private void OnLoadGame()
        {
            if (LoadGame != null)
                LoadGame(this, EventArgs.Empty);
        }

        /// <summary>
        /// Játék mentése eseménykiváltása.
        /// </summary>
        private void OnSaveGame()
        {
            if (SaveGame != null)
                SaveGame(this, EventArgs.Empty);
        }

        /// <summary>
        /// Játék szüneteltetésének eseménykiváltása.
        /// </summary>
        private void OnPauseGame()
        {
            if (PauseGame != null)
                PauseGame(this, EventArgs.Empty);
        }

        /// <summary>
        /// Játékból való kilépés eseménykiváltása.
        /// </summary>
        private void OnExitGame()
        {
            if (ExitGame != null)
                ExitGame(this, EventArgs.Empty);
        }

        private void RightPressed()
        {
            if (Right != null)
                Right(this, EventArgs.Empty);
            //_model.GoRight();
        }

        private void LeftPressed()
        {
            if (Left != null)
                Left(this, EventArgs.Empty);
            //_model.GoLeft();
        }
        private void UpPressed()
        {
            if (Up != null)
                Up(this, EventArgs.Empty);
            //_model.RotateMeSenpai();
        }
        private void DownPressed()
        {
            if (Down != null)
                Down(this, EventArgs.Empty);
            //_model.GoDown();
        }

        #endregion
    }
}
