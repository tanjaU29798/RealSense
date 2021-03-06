﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace RealSense
{
    /**
     * abstract module to implment the Init method of the model and contains the work method
     * 
     * @Author Tanja Witke
     */
    public abstract class RSModule
    {
        protected double MIN = 0, MAX = 0, MIN_TOL, MAX_TOL, XTREME_MAX, XTREME_MIN /*dayum*/;
        protected static double DEF_MIN, DEF_MAX;
        protected static Model model;
        public String output = "";
        protected bool debug = false;
        protected static int numFramesBeforeAccept = 20;
        protected int framesGathered = 0;
        public int[] triggers = { };

        /**
        * initialise the model
        * @param m - the model
        */
        public static void Init(Model m)
        {
         //   System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace();
         //   Console.WriteLine(t.ToString());
            model = m;
        }

        /**
         * Custom method when triggered
         * @param int key - key to trigger this module
         **/
        public virtual void KeyTrigger(int key)
        {
            //can be overriden
        }

        /**
         *  Update every frame (do calculations, manipulate output Image)
         *  @param Graphics g
         */
        public abstract void Work(Graphics g);

        /**
         * Resets the Min and the Max value.
         * @param double[] vars - values to consider
         * @return ret converted values
         * */
        protected double[] ConvertValues(double[] vars)
        {
            double[] ret = new double[vars.Length];

            for (int i = 0; i < vars.Length; i++)
            {
                if (vars[i] >= 0)
                    ret[i] = vars[i] * 100 / MAX;
                else
                    ret[i] = vars[i] * 100 / -MIN;
            }

            return ret;
        }

        /**
         * Called in DynamicMinMax
         * @param double[] vars - values to consider
         * @return average Average of filtered Values
         */
        protected double FilteredAvg(double[] values)
        {
            double average = 0, numAverages = 0;
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] > XTREME_MAX) average += XTREME_MAX;
                else if (values[i] < XTREME_MIN) average += XTREME_MIN;
                else average += values[i];
                numAverages++;
            }

            average /= numAverages;

            return average;
        }

        /**
         * Removes values that fall within the tolerance-spectrum.
         * @param double[] vars - values to consider
         * */
        protected void FilterToleranceValues(double[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = values[i] < MAX_TOL && values[i] > MIN_TOL ? 0 : values[i];
            }
        }

        /**
         * Puts a new Min/Max value if the dist is higher/lower than the old one.
         * @param dist value to compare
         * */
        protected void DynamicMinMax(double[] dist)
        {
            if (model.CurrentPoseDiff > model.PoseMax)
            { output = ""; return; }
            double temp = dist.Min();
            MIN = MIN < temp ? MIN : temp * 0.9;
            temp = dist.Max();
            //   Console.WriteLine("\n" + MIN + ", " + MAX);
            MAX = MAX < temp ? temp * 0.9 : MAX;
        }

        /**
         * Reset value boundaries
         */ 
        public void Reset()
        {
            MIN = DEF_MIN;
            MAX = DEF_MAX;
        }

        /**
         * Getter of the debug value
         * 
         * */
        public bool Debug
        {
            get { return debug; }
            set { debug = value; }
        }
    }

}
