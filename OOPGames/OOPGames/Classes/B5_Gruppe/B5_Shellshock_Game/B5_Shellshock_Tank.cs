using System;

namespace OOPGames
{
    // Represents a single tank. Stores position, shooting angle, power, and health status.
    public class B5_Shellshock_Tank
    {
        private double _x;
        private double _y;
        private double _angle;
        private double _power;
        private int _health;
        private B5_Shellshock_TankColor _color;

        public double X
        {
            get => _x;
            set => _x = value;
        }

        public double Y
        {
            get => _y;
            set => _y = value;
        }

        public double Angle
        {
            get => _angle;
            set => _angle = Math.Max(0, Math.Min(180, value)); // Clamp between 0 and 180
        }

        public double Power
        {
            get => _power;
            set => _power = Math.Max(0, Math.Min(100, value)); // Clamp between 0 and 100
        }

        public int Health
        {
            get => _health;
            set => _health = Math.Max(0, value);
        }

        public B5_Shellshock_TankColor Color => _color;

        public bool IsAlive => _health > 0;

        public B5_Shellshock_Tank(double x, double y, B5_Shellshock_TankColor color)
        {
            _x = x;
            _y = y;
            _color = color;
            _angle = 45; // Default angle
            _power = 50; // Default power
            _health = 100; // Default health
        }

        public void TakeDamage(int damage)
        {
            _health = Math.Max(0, _health - damage);
        }

        public void MoveLeft(double distance)
        {
            _x -= distance;
        }

        public void MoveRight(double distance)
        {
            _x += distance;
        }

        public void AdjustAngle(double delta)
        {
            Angle = _angle + delta;
        }

        public void AdjustPower(double delta)
        {
            Power = _power + delta;
        }
    }
}
