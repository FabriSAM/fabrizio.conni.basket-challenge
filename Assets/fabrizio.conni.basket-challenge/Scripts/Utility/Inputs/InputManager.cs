using UnityEngine;

namespace FabrizioConni.BasketChallenge.Utility
{
    public static class InputManager
    {

        private static Inputs input;

        static InputManager()
        {
            input = new Inputs();

            input.Computer.Enable();
        }
        
        public static Inputs.ComputerActions Computer { get { return input.Computer; } }

        public static Vector2 MousePosition { get { return input.Computer.MousePosition.ReadValue<Vector2>(); } }

        public static bool Computer_Shoot_IsPressed() { return input.Computer.Shoot.IsPressed(); }

        

        public static void EnableComputerInput(bool enable)
        {
            if (enable)
                input.Computer.Enable();
            else
                input.Computer.Disable();
        }

        
    }
}
