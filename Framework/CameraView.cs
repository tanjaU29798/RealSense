﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.Drawing.Imaging;

namespace RealSense
{
    /**
     * Main-UI for testing- and presentation-purposes
     * @author: David Rosenbusch
     * @HogwartsHouse Hufflepuff
     * 
     */
    public class CameraView : Form
    {
        // all the things 
        private PXCMSession session;
        private PXCMSession.ImplVersion iv;
        // Used to store Image Data and convert to bitmap
        private PXCMImage.ImageData colorData;
        // the bitmap that we put into the pictureBox
        private Bitmap colorBitmap;
        // the PictureBox that we put into the window (this class)
        private PictureBox pb;
        // running number to Save all the images to the hard drive (careful with that ;) )
        private Thread updaterThread;
        private Model model;
        private   int save = 0, debug_y = 0;
        private Button enableOutput = new Button();
        private Button enableImage = new Button();
        private bool outputEnabled, imageEnabled = true, resetModules = false;
        private bool testMode, blur = true;
        private Bitmap uiBitmap, windowBitmap, smallWindowBitmap, warningBitmap;
        private  int subject = 0;


        static int xgap = (int)(75 * 5);
        static int ygap = (int)(80 * 2.5 + 20);
        static int yRingGap = 20;
        static int targetX = 120;
        static int xP = -900, yP = 160, yV = 90;
        static int thickness = 25;
        static int radius = 70;
        static int warningX = xP - 70, warningY = yP - 130;
        static int warningWidth = 900, warningHeight = 1025;
        static int warningSymbolX = (warningX + warningWidth) / 2 - 728 / 2, warningSymbolY = (warningY + warningHeight) / 2 - 720 / 2;
        static int warningPopupX = 2500, warningPopupSlideX = 1420;
        static bool uiSlide = false;

        private FriggnAweseomeGraphix.MEMonitor angerMonitor = new FriggnAweseomeGraphix.MEMonitor("Anger", "Wut", xP, yP + yRingGap, radius, thickness);
        private FriggnAweseomeGraphix.MEMonitor joyMonitor = new FriggnAweseomeGraphix.MEMonitor("Joy", "Freude", xP, yP + yRingGap + ygap, radius, thickness);
        private FriggnAweseomeGraphix.MEMonitor fearMonitor = new FriggnAweseomeGraphix.MEMonitor("Fear", "Furcht", xP, yP + yRingGap + ygap * 2, radius, thickness);
        private FriggnAweseomeGraphix.MEMonitor contemptMonitor = new FriggnAweseomeGraphix.MEMonitor("Contempt", "Verachtung", xP, yP + yRingGap + ygap * 3, radius, thickness);

        private FriggnAweseomeGraphix.MEMonitor sadMonitor = new FriggnAweseomeGraphix.MEMonitor("Sadness", "Trauer", xP + xgap, yP + yRingGap + yV, radius, thickness);
        private FriggnAweseomeGraphix.MEMonitor disgustMonitor = new FriggnAweseomeGraphix.MEMonitor("Disgust", "Ekel", xP + xgap, yP + yRingGap + yV + ygap, radius, thickness);
        private FriggnAweseomeGraphix.MEMonitor surpriseMonitor = new FriggnAweseomeGraphix.MEMonitor("Surprise", "Überraschung", xP + xgap, yP + yRingGap + yV + ygap * 2, radius, thickness);
        int calibRadius = 300;

        private Pen linePen = new Pen(new SolidBrush(Color.Gray));

        /**
         * Initialises View and starts updater Thread
         * @param model - reference to data model
         * @param Mode.Mode m - mode the UI should run
         */
        public CameraView(Model model, Model.MODE m)
        {
            KeyPreview = true;

            this.model = model;
            model.View = this;
            testMode = m == Model.MODE.TEST;
            session = PXCMSession.CreateInstance();

            if (session == null) // Something went wrong, session could not be initialised
            {
                Application.Exit();
                return;
            }

            iv = session.QueryVersion();
            String versionString = "v" + iv.major + "." + iv.minor;
            Text = versionString;


            pb = new PictureBox();
            FormClosed += new FormClosedEventHandler(Quit);

            if ((m == Model.MODE.TEST))
            {
                // Set size
                pb.Bounds = new Rectangle(0, 0, model.Width, model.Height);
                // init UI
                this.Bounds = new Rectangle(0, 0, model.Width, model.Height + 180);
                this.Controls.Add(pb);

                enableOutput.Bounds = new Rectangle(20, 1080, 500, 30);
                enableOutput.Text = "Output";
                enableOutput.Click +=
                    new System.EventHandler(delegate
                    {
                        outputEnabled = !outputEnabled;
                    });
                AddComponent(enableOutput);

                enableImage.Bounds = new Rectangle(20, 1110, 500, 30);
                enableImage.Text = "NoImg";
                enableImage.Click +=
                    new System.EventHandler(delegate
                    {
                        imageEnabled = !imageEnabled;
                    });
                AddComponent(enableImage);
            }
            else
            {
                windowBitmap = new Bitmap(Bitmap.FromFile(Directory.GetCurrentDirectory() +"\\Images\\window.png"));
                smallWindowBitmap = new Bitmap(Bitmap.FromFile(Directory.GetCurrentDirectory()+"\\Images\\small_window.png"));
                warningBitmap = new Bitmap(Bitmap.FromFile(Directory.GetCurrentDirectory()+"\\Images\\warning.png"));
                this.Bounds = Screen.PrimaryScreen.Bounds;
                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Maximized;
                pb.Bounds = new Rectangle(this.Bounds.Width / 2 - model.Width / 2, this.Bounds.Height / 2 - model.Height / 2, 1920, 1080);
                this.Controls.Add(pb);
                this.BackColor = Color.Black;
            }
            KeyDown += OnKeyDown;
            this.Show();
            // Start Updater Thread
            updaterThread = new Thread(this.update);
            updaterThread.Start();
        }

        /**
         * Key-Listener to trigger modules
         * 
         * @param object sender, sending component
         * @param KeyEventArgs e, Key-Event information
         * 
         */
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                if (e.KeyValue == (int)Keys.C)
                {
                    model.Modules.ForEach(delegate (RSModule mod)
                    {
                        if (mod.GetType() == typeof(Gauge_Module))
                        {

                            model.Modules.ForEach(delegate (RSModule md)
                            {
                            });
                            Console.WriteLine("Start calibration");
                            ((Gauge_Module)mod).calibrate = true;
                            subject++;
                        }
                    });
                    ResetModules = true;

                }
                else if (e.KeyValue == (int)Keys.B)
                {
                    blur = !blur;
                }
                else if (e.KeyValue == (int)Keys.R)
                {
                    model.Modules.ForEach(delegate (RSModule mod)
                    {
                        mod.Reset();
                    });
                }
            }
            model.Modules.ForEach(delegate (RSModule mod)
            {
                foreach (int i in mod.triggers)
                {
                    if (i == e.KeyValue)
                    {
                        mod.KeyTrigger(e.KeyValue);
                        return;
                    }
                }
            });
        }

        /**
         * Appropriately close down app 
         * 
         * @param object sender, sending component
         * @param KeyEventArgs e, Key-Event information
         * 
         */
        private void Quit(object sender, FormClosedEventArgs e)
        {
            updaterThread.Abort();
            session.Dispose();
            model.SenseManager.Dispose();
            Application.Exit();
        }

        /**
         * Add a component to the UI (thread-safe)
         * 
         * @param Control c, control to be added
         */
        public void AddComponent(Control c)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    AddComponent(c);
                });
            }
            else
            {
                this.Controls.Add(c);
            }
        }

        /**
         * Update the View and modules
         */
        private void update()
        {
            Stopwatch stopwatch = new Stopwatch();
            while (true)
            {
                if (model.SenseManager.AcquireFrame(true) >= pxcmStatus.PXCM_STATUS_NO_ERROR) // Dauert manchmal voll lange ...
                {
                    debug_y = 0;
                    // <magic>
                    PXCMCapture.Sample sample = model.SenseManager.QueryFaceSample();
                    sample.color.AcquireAccess(PXCMImage.Access.ACCESS_READ, PXCMImage.PixelFormat.PIXEL_FORMAT_RGB32, out colorData);

                    model.FaceData = model.Face.CreateOutput();
                    model.FaceData.Update();
                    model.FaceAktuell = model.FaceData.QueryFaceByIndex(0);
                    if (model.FaceAktuell != null)
                    {
                        uiSlide = true;
                        PXCMFaceData.PoseData pose = model.FaceAktuell.QueryPose();
                        if (pose != null)
                            pose.QueryPoseAngles(out model.currentPose);
                        model.Lp = model.FaceAktuell.QueryLandmarks();
                        if (model.NullFace == null)
                        {
                            if (model.Lp != null)
                            {
                                PXCMFaceData.LandmarkPoint[] aPoints;
                                model.Lp.QueryPoints(out aPoints);
                                model.NullFace = aPoints;
                            }
                        }


                    }

                    colorBitmap = colorData.ToBitmap(0, sample.color.info.width, sample.color.info.height);

                    Graphics bitmapGraphics = Graphics.FromImage(colorBitmap);
                    if (resetModules)
                    {
                        model.Modules.ForEach(delegate (RSModule mod)
                        {
                            mod.Reset();
                        });
                        resetModules = false;
                    }

                    if (testMode)
                    {
                        if (outputEnabled)
                            bitmapGraphics.FillRectangle(model.DefaultBGBrush, new Rectangle(0, 0, model.Width, model.Height));
                        if (!imageEnabled)
                            bitmapGraphics.FillRectangle(model.OpaqueBGBrush, new Rectangle(0, 0, model.Width, model.Height));

                        bitmapGraphics.DrawString("pose: " + model.CurrentPoseDiff, model.DefaultFont, model.DefaultStringBrush, 10, Debug_Y);
                        Debug_Y += 25;
                    }
                    else
                    {
                        using (Graphics gr = Graphics.FromImage(colorBitmap))
                        {
                            if (model.calibrationProgress == 100)
                            {
                                angerMonitor.targetValue = (int)model.Emotions[Model.Emotion.ANGER];
                                fearMonitor.targetValue = (int)model.Emotions[Model.Emotion.FEAR];
                                disgustMonitor.targetValue = (int)model.Emotions[Model.Emotion.DISGUST];
                                surpriseMonitor.targetValue = (int)model.Emotions[Model.Emotion.SURPRISE];
                                joyMonitor.targetValue = (int)model.Emotions[Model.Emotion.JOY];
                                sadMonitor.targetValue = (int)model.Emotions[Model.Emotion.SADNESS];
                                contemptMonitor.targetValue = (int)model.Emotions[Model.Emotion.CONTEMPT];

                                angerMonitor.Step();
                                fearMonitor.Step();
                                disgustMonitor.Step();
                                surpriseMonitor.Step();
                                joyMonitor.Step();
                                sadMonitor.Step();
                                contemptMonitor.Step();
                            }

                            if (!testMode)
                            {
                                gr.DrawImage(windowBitmap, xP - 90, yP - 150);
                            }
                            FriggnAweseomeGraphix.DrawMEMontior(gr, angerMonitor);
                            FriggnAweseomeGraphix.DrawMEMontior(gr, sadMonitor);
                            FriggnAweseomeGraphix.DrawMEMontior(gr, fearMonitor);
                            FriggnAweseomeGraphix.DrawMEMontior(gr, surpriseMonitor);
                            FriggnAweseomeGraphix.DrawMEMontior(gr, contemptMonitor);
                            FriggnAweseomeGraphix.DrawMEMontior(gr, disgustMonitor);
                            FriggnAweseomeGraphix.DrawMEMontior(gr, joyMonitor);
                            bitmapGraphics.DrawString("Subject #" + subject, FriggnAweseomeGraphix.majorFont, new SolidBrush(FriggnAweseomeGraphix.fontColor), xP, yP - 75);
                            bitmapGraphics.DrawLine(linePen, xP + 10, yP - 20, xP + 800, yP - 20);
                            bitmapGraphics.DrawString("Pose: " + (int)model.CurrentPoseDiff, FriggnAweseomeGraphix.minorFont, new SolidBrush(FriggnAweseomeGraphix.fontColor), xP + 550, yP - 55);

                            FriggnAweseomeGraphix.MEMonitor calibMonitor = new FriggnAweseomeGraphix.MEMonitor("", "", 1150, 580 - calibRadius, calibRadius, 20);
                            calibMonitor.showPercent = false;
                            if (model.calibrationProgress != 100 && model.CurrentFace != null)
                            {
                                PXCMFaceData.LandmarkPoint mPoint = model.CurrentFace[29];

                                int rad = 2;

                                if (mPoint.image.x > 1200)
                                {
                                    calibMonitor.y = (int)mPoint.image.y - calibRadius;
                                    calibMonitor.x = (int)mPoint.image.x - calibRadius;
                                }

                                calibMonitor.showPercent = false;
                                calibMonitor.currentValue = (int)model.calibrationProgress;
                                int sMark = (int)(model.calibrationProgress * 0.69);
                                int tMark = sMark + 1;


                                PXCMFaceData.LandmarkPoint sPoint = model.CurrentFace[sMark];
                                PXCMFaceData.LandmarkPoint tPoint = model.CurrentFace[tMark];


                                SolidBrush sb = new SolidBrush(FriggnAweseomeGraphix.fgColor);
                                for (int i = 0; i < sMark - 1; i++)
                                {
                                    PXCMFaceData.LandmarkPoint iPoint = model.CurrentFace[i];
                                    gr.FillEllipse(sb, new Rectangle((int)iPoint.image.x - rad, (int)iPoint.image.y - rad, rad * 2, rad * 2));
                                }

                                gr.FillEllipse(sb, new Rectangle((int)sPoint.image.x - rad, (int)sPoint.image.y - rad, rad * 4, rad * 4));
                                FriggnAweseomeGraphix.DrawMEMontior(gr, calibMonitor);
                            }
                            else calibMonitor.currentValue = 0;
                            if (model.CurrentPoseDiff > model.PoseMax && model.calibrationProgress == 100)
                            {
                                warningPopupX += (warningPopupSlideX - warningPopupX) / 3;
                                gr.FillRectangle(new SolidBrush(Color.FromArgb(100, 255, 255, 255)), new Rectangle(warningX, warningY, warningWidth, warningHeight));
                                gr.DrawImage(smallWindowBitmap, warningPopupX, 900);



                            }
                            else warningPopupX = 2500;
                        }

                    }


                    if (model.FaceData.QueryNumberOfDetectedFaces() > 0 && model.CurrentFace != null)
                    {
                        model.Modules.ForEach(delegate (RSModule mod)
                        {
                            mod.Work(bitmapGraphics);
                            if (outputEnabled && mod.output != "")
                            {
                                bitmapGraphics.DrawString(mod.output, model.DefaultFont, model.DefaultStringBrush, 10, Debug_Y);
                                Debug_Y += 25; // new row
                            }
                        });
                    }

                    double pitchDiff = Math.Abs(model.currentPose.pitch - model.NullPose.pitch);
                    double rollDiff = Math.Abs(model.currentPose.roll - model.NullPose.roll);
                    double yawDiff = Math.Abs(model.currentPose.yaw - model.NullPose.yaw);

                    model.CurrentPoseDiff = pitchDiff + rollDiff + yawDiff;
                    model.CurrentRollDiff = rollDiff;
                    model.CurrentPitchDiff = pitchDiff;
                    model.CurrentYawDiff = yawDiff;

                    // Update PictureBox
                    if (testMode) pb.Image = colorBitmap;
                    else pb.Image = colorBitmap;// uiBitmap.Clone(new Rectangle(0, 0, uiBitmap.Width, uiBitmap.Height), uiBitmap.PixelFormat);
                    if (PXCMCapture.Device.MirrorMode.MIRROR_MODE_HORIZONTAL != model.SenseManager.captureManager.device.QueryMirrorMode()) model.SenseManager.captureManager.device.SetMirrorMode(PXCMCapture.Device.MirrorMode.MIRROR_MODE_HORIZONTAL); //mirror
                    model.SenseManager.ReleaseFrame();
                    model.FaceData.Dispose(); // DONE!
                    sample.color.ReleaseAccess(colorData);
                    if (xP < targetX && uiSlide)
                    {
                        xP += (targetX - xP) / 8;
                        warningX = xP - 70;
                        warningSymbolX = (warningX + warningWidth) / 2 - 728 / 2;
                        angerMonitor.x = xP;
                        joyMonitor.x = xP;
                        fearMonitor.x = xP;
                        contemptMonitor.x = xP;

                        sadMonitor.x = xP + xgap;
                        disgustMonitor.x = xP + xgap;
                        surpriseMonitor.x = xP + xgap;
                    }
                }
            }
        }
        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(282, 253);
            this.Name = "CameraView";
            this.ResumeLayout(false);

        }

        /**
         * Current y-coordinate of debug-messages
         */
        public int Debug_Y
        {
            get { return debug_y; }
            set { debug_y = value; }
        }

        public Boolean ResetModules
        {
            set { resetModules = value; }
        }

        public Bitmap ColorBitmap
        {
            get { return colorBitmap; }
        }


    }
}