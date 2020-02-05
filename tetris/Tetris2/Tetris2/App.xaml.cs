using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Windows;
using System.ComponentModel;
using System.Windows.Threading;
using Tetris.Model;
using Tetris.Persistence;
using Tetris2.ViewModel;

namespace Tetris2
{

    public partial class App : Application
    {
        private TetrisModel _model;
        private GameViewModel _viewModel;
        private MainWindow _view;
        private LoadWindow _loadWindow;
        private SaveWindow _saveWindow;
        private DispatcherTimer _timer;
        private Boolean _timerActive;

        public App()
        {
            Startup += new StartupEventHandler(App_Startup);
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            TetrisDataAccessInterface dataAccess;

            dataAccess = new TetrisDbDataAccess("name=TetrisModel");

            // modell létrehozása
            _model = new TetrisModel(dataAccess);
            _model.GameOver += new EventHandler<TetrisEventArgs>(Model_GameEnd); // késöbb megírni
            _model.NewGame();

            // nézemodell létrehozása
            _viewModel = new GameViewModel(_model);
            _viewModel.NewGame += new EventHandler(ViewModel_NewGame);
            _viewModel.ExitGame += new EventHandler(ViewModel_ExitGame);
            _viewModel.PauseGame += new EventHandler(ViewModel_PauseGame);
            _viewModel.SmallGame += new EventHandler(ViewModel_SmallGame);
            _viewModel.MediumGame += new EventHandler(ViewModel_MediumGame);
            _viewModel.LargeGame += new EventHandler(ViewModel_LargeGame);
            _viewModel.LoadGameOpen += new EventHandler(ViewModel_LoadGameOpen);
            _viewModel.LoadGameClose += new EventHandler<String>(ViewModel_LoadGameClose);
            _viewModel.SaveGameOpen += new EventHandler(ViewModel_SaveGameOpen);
            _viewModel.SaveGameClose += new EventHandler<String>(ViewModel_SaveGameClose);

            _viewModel.Down += new EventHandler(ViewModel_Down);
            _viewModel.Up += new EventHandler(ViewModel_Up);
            _viewModel.Right += new EventHandler(ViewModel_Right);
            _viewModel.Left += new EventHandler(ViewModel_Left);

            // nézet létrehozása
            _view = new MainWindow();
            _view.DataContext = _viewModel;
            _view.Closing += new System.ComponentModel.CancelEventHandler(View_Closing); // eseménykezelés a bezáráshoz
            _view.Show();

            // időzítő létrehozása
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += new EventHandler(Timer_Tick);
            _timer.Start();
            _timerActive = true;


        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _model.AdvanceTime();
        }

        #region View event handlers

        /// <summary>
        /// Nézet bezárásának eseménykezelője.
        /// </summary>
        private void View_Closing(object sender, CancelEventArgs e)
        {
            Boolean restartTimer = _timer.IsEnabled;

            _timer.Stop();

            if (MessageBox.Show("Biztos, hogy ki akar lépni?", "Game", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                e.Cancel = true; // töröljük a bezárást

                if (restartTimer) // ha szükséges, elindítjuk az időzítőt
                    _timer.Start();
            }
        }

        #endregion

        #region ViewModel event handlers

        /// <summary>
        /// Új játék indításának eseménykezelője.
        /// </summary>
        private void ViewModel_NewGame(object sender, EventArgs e)
        {
            _model.NewGame();
            _viewModel.Rows = 8;
            _viewModel.WindowHeight = 400;
            _timer.Start();
            _timerActive = true;
        }

        private void ViewModel_SmallGame(object sender, EventArgs e)
        {
            _model.SetTableSize(0);
            _model.NewGame();
            _viewModel.Rows = 4;
            _viewModel.WindowHeight = 250;
            _timer.Start();
            _timerActive = true;
        }

        private void ViewModel_MediumGame(object sender, EventArgs e)
        {
            _model.SetTableSize(1);
            _model.NewGame();
            _viewModel.Rows = 8;
            _viewModel.WindowHeight = 450;
            _timer.Start();
            _timerActive = true;
        }

        private void ViewModel_LargeGame(object sender, EventArgs e)
        {
            _model.SetTableSize(2);
            _model.NewGame();
            _viewModel.Rows = 12;
            _viewModel.WindowHeight = 650;
            _timer.Start();
            _timerActive = true;
        }
        private void ViewModel_LoadGameOpen(object sender, System.EventArgs e)
        {
            Boolean restartTimer = _timer.IsEnabled;

            _timer.Stop();

            _viewModel.SelectedGame = null; // kezdetben nincsen kiválasztott elem

            _loadWindow = new LoadWindow(); // létrehozzuk a játék állapot betöltő ablakot
            _loadWindow.DataContext = _viewModel;
            _loadWindow.ShowDialog(); // megjelenítjük dialógusként

            if (restartTimer) // ha szükséges, elindítjuk az időzítőt
                _timer.Start();
        }

        /// <summary>
        /// Játék betöltésének eseménykezelője.
        /// </summary>
        private async void ViewModel_LoadGameClose(object sender, String name)
        {
            if (name != null)
            {
                try
                {
                    await _model.LoadGameAsync(name);
                }
                catch
                {
                    MessageBox.Show("Játék betöltése sikertelen!", "Hiba!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            Int32 a = _model.getTableSize();
            switch (a)
            {
                case 4:
                    _viewModel.Rows = 4;
                    _viewModel.WindowHeight = 250;
                    break;
                case 8:
                    _viewModel.Rows = 8;
                    _viewModel.WindowHeight = 450;
                    break;
                case 12:
                    _viewModel.Rows = 12;
                    _viewModel.WindowHeight = 650;
                    break;
                default:
                    _viewModel.Rows = 8;
                    _viewModel.WindowHeight = 450;
                    break;
            }
            _timer.Start();
            _loadWindow.Close(); // játékállapot betöltőtő ablak bezárása
        }

        /// <summary>
        /// Játék mentés választó eseménykezelője.
        /// </summary>
        private void ViewModel_SaveGameOpen(object sender, EventArgs e)
        {
            Boolean restartTimer = _timer.IsEnabled;

            _timer.Stop();

            _viewModel.SelectedGame = null; // kezdetben nincsen kiválasztott elem
            _viewModel.NewName = String.Empty;

            _saveWindow = new SaveWindow(); // létrehozzuk a játék állapot mentő ablakot
            _saveWindow.DataContext = _viewModel;
            _saveWindow.ShowDialog(); // megjelenítjük dialógusként

            if (restartTimer) // ha szükséges, elindítjuk az időzítőt
                _timer.Start();
        }

        /// <summary>
        /// Játék mentésének eseménykezelője.
        /// </summary>
        private async void ViewModel_SaveGameClose(object sender, String name)
        {
            if (name != null)
            {
                
                try
                {
                    // felülírás ellenőrzése
                    var games = await _model.ListGamesAsync();
                    if (games.All(g => g.Name != name) ||
                        MessageBox.Show("Biztos, hogy felülírja a meglévő mentést?", "Tetris",
                            MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        await _model.SaveGameAsync(name);
                    }
                }
                catch
                {
                    MessageBox.Show("Játék mentése sikertelen!", "Hiba!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            _saveWindow.Close(); // játékállapot mentő ablak bezárása
        }

        /// <summary>
        /// Játékból való kilépés eseménykezelője.
        /// </summary>
        private void ViewModel_ExitGame(object sender, System.EventArgs e)
        {
            _view.Close(); // ablak bezárása
        }

        private async void ViewModel_LoadGame(object sender, System.EventArgs e)
        {
            Boolean restartTimer = _timer.IsEnabled;
            _timer.Stop();

            OpenFileDialog openFileDialog = new OpenFileDialog(); // dialógusablak
            openFileDialog.Title = "Tetris table load";
            openFileDialog.Filter = "Tetris table|*.tt";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    await _model.LoadGameAsync(openFileDialog.FileName);
                    //saveToolStripMenuItem.Enabled = true;
                }
                catch (TetrisDataException)
                {
                    MessageBox.Show("Couln't load the game!", "Game", MessageBoxButton.OK, MessageBoxImage.Error);

                }

            }

            if (restartTimer)
                _timer.Start();
        }

        private async void ViewModel_SaveGame(object sender, System.EventArgs e)
        {
            Boolean restartTimer = _timer.IsEnabled;
            _timer.Stop();

            SaveFileDialog saveFileDialog = new SaveFileDialog(); // dialógablak
            saveFileDialog.Title = "Tetris table save";
            saveFileDialog.Filter = "Tetris table|*.tt";

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    await _model.SaveGameAsync(saveFileDialog.FileName);
                }
                catch (TetrisDataException)
                {
                    MessageBox.Show("Couldn't save the game!" + Environment.NewLine + "Wrong path or you don't have the permission to write to the directory", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception)
                {
                    MessageBox.Show("Couldn't save the game!" + Environment.NewLine + "You can't save a lost game!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            if (restartTimer)
                _timer.Start();
        }

        private void ViewModel_PauseGame(object sender, System.EventArgs e)
        {
            if (!_model.isLost)
            {
                if (_timerActive)
                {
                    _timer.Stop();
                    _timerActive = false;
                }
                else
                {
                    _timer.Start();
                    _timerActive = true;
                }
            }
        }

        private void ViewModel_Down(object sender, EventArgs e)
        {
            if (_timerActive)
                _model.GoDown();
        }
        private void ViewModel_Up(object sender, EventArgs e)
        {
            if (_timerActive)
                _model.RotateMeSenpai();
        }
        private void ViewModel_Right(object sender, EventArgs e)
        {
            if (_timerActive)
                _model.GoRight();
        }
        private void ViewModel_Left(object sender, EventArgs e)
        {
            if (_timerActive)
                _model.GoLeft();
        }




        #endregion

        #region Model event handlers

        /// <summary>
        /// Játék végének eseménykezelője.
        /// </summary>
        private void Model_GameEnd(object sender, TetrisEventArgs e)
        {
            _timer.Stop();
            _timerActive = false;

            if (e.returnIsLost)
            {
                MessageBox.Show("Gratulálok, győztél!" + Environment.NewLine +
                                "Összesen " + TimeSpan.FromSeconds(e.ReturnGameTime).ToString("g") + " ideig játszottál.",
                                "Game játék",
                                MessageBoxButton.OK,
                                MessageBoxImage.Asterisk);
            }
        }

        #endregion
    }
}
