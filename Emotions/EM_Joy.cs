﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace RealSense.Emotions
{

    /**
 *Measures the percentage value of joy. 
 *@author Tanja 
 */
    class EM_Joy : RSModule
    {
        int percent = 100;
        double[] smallerArray;

        /**
       * Initializes the EM, setting the debug-flag to true by default
       */
        public EM_Joy()
        {
            debug = true;
        }

        /**
         * Computes the percentage Value of Joy in the current Frame.
         * @param Graphics g for the view
         * */
        public override void Work(Graphics g)
        {
            //Joy --> EyelidTight, LipCorner

            //percentage Joy
            int p_lid = 20;
            int p_lip = 80;

            Reduce();

            //lid Value 0 - -100 (Grenze bei lidMax)
            double temp_left = model.AU_Values[typeof(AU_EyelidTight).ToString() + "_left"];
            double temp_right = model.AU_Values[typeof(AU_EyelidTight).ToString() + "_right"];
            double lidValue = (temp_left + temp_right)  / 2;
            lidValue = lidValue * -1 * p_lid / percent;

            //lip Value 0 - 100
            temp_left = Math.Abs(model.AU_Values[typeof(AU_LipCorner).ToString() + "_left"]);
            temp_right = Math.Abs(model.AU_Values[typeof(AU_LipCorner).ToString() + "_right"]);
            double lipValue = temp_left > temp_right ? temp_right : temp_left;
            //lipValue = (temp_left + temp_right) / 2;
            lipValue = lipValue * p_lip / percent;

            //lipL Value 0 - -100
            double lipLValue = model.AU_Values[typeof(AU_LipLine).ToString()];
            lipLValue = lipLValue * p_lip / percent;

            lipValue = lipValue > lipLValue ? lipValue : lipLValue;

            double joy = lidValue + lipValue;// + browValue;

            joy = joy > 0 ? joy : 0;

            model.Emotions[Model.Emotion.JOY] = joy;

            // print debug-values 
            if (debug)
            {
                //output = "Joy: " + (int)joy + " LipCorner: " + (int)lipValue + " LipLine: " + (int)lipLValue + " Eye: " + (int)lidValue; // + " Brow: " + browValue;
            }

        }

        /**
        * Reduces the value boundaries of the emotion value. 
        * A reduced value doesn't reach the 100 anymore. This is happening if an AU_value is active that doesn't match with this emotion.
        *  
        * */
        private void Reduce()
        {
            //Anger brows
            double temp_left = model.AU_Values[typeof(AU_BrowShift).ToString() + "_left"];
            double temp_right = model.AU_Values[typeof(AU_BrowShift).ToString() + "_right"];
            double browValue = (temp_left + temp_right) / 2;
            browValue = browValue * -1 - 30;

            //Disgust nose 
            double noseValue = model.AU_Values[typeof(AU_NoseWrinkled).ToString()];
            noseValue = noseValue * -1;

            //Sadness Lipline
            double lipLValue = model.AU_Values[typeof(AU_LipLine).ToString()];
            lipLValue = lipLValue * -1;

            //Contempt
            //not possible yet (ME needed)

            //Surprise Lid
            temp_left = model.AU_Values[typeof(AU_EyelidTight).ToString() + "_left"];
            temp_right = model.AU_Values[typeof(AU_EyelidTight).ToString() + "_right"];
            double eyeValue = (temp_left + temp_right) / 2;

            smallerArray = new double[] { browValue, lipLValue, eyeValue };
            percent = 100 + (int)(2 * smallerArray.Max());
            percent = percent < 100 ? 100 : percent;

            if (debug)
            {
                //output = "Joy: " + (int)joy + " LipCorner: " + (int)lipValue + " LipLine: " + (int)lipLValue + " Eye: " + (int)lidValue; // + " Brow: " + browValue;
                output = " Smaller: " + percent + " brow: " + (int)browValue + " Lip: " + (int)lipLValue + " Lid: " + (int)eyeValue;
            }

        }
    }
}






