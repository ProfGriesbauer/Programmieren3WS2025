using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOPGames
{
    public interface IB5_TicTacToeField : IGameField
    {
        int GetFieldValue(int row, int col);
    }
    public interface IB5_TicTacToeRules : IGameRules
    {
        
    }
}