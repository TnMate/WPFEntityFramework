using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Tetris.Persistence
{
    /// <summary>
    /// Tetris perzisztencia adatbáziskezelő típusa.
    /// </summary>
	public class TetrisDbDataAccess : TetrisDataAccessInterface
    {
		private TetrisContext _context;

        /// <summary>
        /// Konstruktor.
        /// </summary>
        /// <param name="connection">Adatbázis connection string.</param>
        public TetrisDbDataAccess(String connection)
		{
			_context = new TetrisContext(connection);
			_context.Database.CreateIfNotExists(); // adatbázis séma létrehozása, ha nem létezik
		}

	    /// <summary>
	    /// Játékállapot betöltése.
	    /// </summary>
	    /// <param name="name">Név vagy elérési útvonal.</param>
	    /// <returns>A beolvasott játéktábla.</returns>
		public async Task<TetrisTable> LoadAsync(String name)
		{
			try
			{
				Game game = await _context.Games
				    .Include(g => g.Fields)
				    .SingleAsync(g => g.Name == name); // játék állapot lekérdezése
                byte[,] first = new byte[game.TableSize + 1, 16];
                Int32[,] second = new Int32[game.TableSize + 1, 16];
                TetrisTable table = new TetrisTable(game.TableSize, game.GTime, game.ShapeIndex, game.ShapeX, game.ShapeY, game.ShapeR, first, second); // játéktábla modell létrehozása

				foreach (Field field in game.Fields) // mentett mezők feldolgozása
				{
					table.SetValue(field.X, field.Y, field.Value, field.Type);
				}

				return table;
			}
			catch
			{
				throw new TetrisDataException();
			}
		}

	    /// <summary>
	    /// Játékállapot mentése.
	    /// </summary>
	    /// <param name="name">Név vagy elérési útvonal.</param>
	    /// <param name="table">A kiírandó játéktábla.</param>
		public async Task SaveAsync(String name, TetrisTable table)
		{
            try
			{
                // játékmentés keresése azonos névvel
			    Game overwriteGame = await _context.Games
			        .Include(g => g.Fields)
			        .SingleOrDefaultAsync(g => g.Name == name);
			    if (overwriteGame != null)
			        _context.Games.Remove(overwriteGame); // törlés

				Game dbGame = new Game
				{
					TableSize = table.Size,
                    ShapeIndex = table.CurrentShapeIndex,
                    ShapeX = table.ShapeCordX,
                    ShapeY = table.ShapeCordY,
                    ShapeR = table.ShapeRotation,
                    GTime = table.Time,
					//RegionSize = table.RegionSize,
					Name = name
				}; // új mentés létrehozása

				for (Int32 i = 0; i < table.Size+1; ++i)
				{
					for (Int32 j = 0; j < 16; ++j)
					{
						Field field = new Field
						{
							X = i,
							Y = j,
							Value = table.Table[i, j],
							Type = table.TypeTable[i, j]
                        };
						dbGame.Fields.Add(field);
					}
				} // mezők mentése

				_context.Games.Add(dbGame); // mentés hozzáadása a perzisztálandó objektumokhoz
				await _context.SaveChangesAsync(); // mentés az adatbázisba
			}
			catch(Exception ex)
			{
				throw new TetrisDataException();
			}
		}

	    /// <summary>
	    /// Játékállapot mentések lekérdezése.
	    /// </summary>
	    public async Task<ICollection<SaveEntry>> ListAsync()
	    {
	        try
	        {
                return await _context.Games
                    .OrderByDescending(g => g.Time) // rendezés mentési idő szerint csökkenő sorrendben
                    .Select(g => new SaveEntry { Name = g.Name, Time = g.Time }) // leképezés: Game => SaveEntry
                    .ToListAsync();
            }
	        catch
	        {
	            throw new TetrisDataException();
	        }
	    }
	}
}
