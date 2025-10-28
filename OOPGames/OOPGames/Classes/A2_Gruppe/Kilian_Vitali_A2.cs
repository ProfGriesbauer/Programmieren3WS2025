using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace OOPGames
{
    public class A2_Painter : IPaintGame
    {
        public string Name => throw new NotImplementedException();

        public void PaintGameField(Canvas canvas, IGameField currentField)
        {
            throw new NotImplementedException();
        }
    }



    public class A2_Rules : IGameRules
    {
        public A2_Rules()
        {
        }
    }

}
