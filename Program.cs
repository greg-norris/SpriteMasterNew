
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.I2c;
using GHIElectronics.TinyCLR.Drivers.BasicGraphics;
using GHIElectronics.TinyCLR.Drivers.SolomonSystech.SSD1306;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.Devices.Pwm;
using GHIElectronics.TinyCLR.Devices.Adc;
using System.Threading;

namespace SpriteMasterNew {
    class Program {
        static BasicGraphics basicGfx;
        static SSD1306Controller lcd;
        static GpioPin lcdReset;
        static int row = 0;
        static Sprite enemy = new Sprite();
        static Sprite enemy2 = new Sprite();
        static Sprite enemy3 = new Sprite();
        static bool enemyAlive = true;
        static bool enemyAlive2 = true;
        static bool enemyAlive3 = true;
        static Sprite bullet = new Sprite();
        static Sprite player = new Sprite();
        static int score = 0;
        static uint white = BasicGraphics.ColorFromRgb(0xff, 0xff, 0xff);
        static uint black = BasicGraphics.ColorFromRgb(0x0, 0x0, 0x0);

        static PwmController buzzerController = PwmController.FromName(SC13048.Timer.Pwm.Controller2.Id);
        static PwmChannel buzzer = buzzerController.OpenChannel(SC13048.Timer.Pwm.Controller2.PA5);
        static GpioPin upButton;
        static GpioPin downButton;
        static GpioPin rightButton;
        static GpioPin leftButton;

        static GpioPin vibrationMotor;
        /// Old Game Variables
        ///  //Game Variables
        static int XShip = 62;
        static int YBullet = 20;
        static int XBullet = 0;
        static bool BulletIsOut = false;
        static int XMonster = 0;
        static int YMonster = 0;
        static int enemySpeed1 = 5;
        static int enemySpeed2 = 5;
        static int enemySpeed3 = 5;

        static int lives = 3;

        static void Main() {
            lcdReset = GpioController.GetDefault().OpenPin(SC13048.GpioPin.PB2);
            lcdReset.SetDriveMode(GpioPinDriveMode.Output);
            lcdReset.Write(GpioPinValue.Low);
            Thread.Sleep(50);
            lcdReset.Write(GpioPinValue.High);
            var i2c = I2cController.FromName(SC13048.I2cBus.I2c2);
            var device = i2c.GetDevice(SSD1306Controller.GetConnectionSettings());
            lcd = new SSD1306Controller(device);
            basicGfx = new BasicGraphics(128, 64, ColorFormat.OneBpp);

            //BrainGammer Controls
            upButton = GpioController.GetDefault().OpenPin(SC13048.GpioPin.PB4);
            upButton.SetDriveMode(GpioPinDriveMode.InputPullUp);
            upButton.ValueChangedEdge = GpioPinEdge.FallingEdge | GpioPinEdge.RisingEdge;

            downButton = GpioController.GetDefault().OpenPin(SC13048.GpioPin.PB5);
            downButton.SetDriveMode(GpioPinDriveMode.InputPullUp);
            downButton.ValueChangedEdge = GpioPinEdge.FallingEdge | GpioPinEdge.RisingEdge;

            leftButton = GpioController.GetDefault().OpenPin(SC13048.GpioPin.PB3);
            leftButton.SetDriveMode(GpioPinDriveMode.InputPullUp);
            leftButton.ValueChangedEdge = GpioPinEdge.FallingEdge | GpioPinEdge.RisingEdge;


            rightButton = GpioController.GetDefault().OpenPin(SC13048.GpioPin.PB12);
            rightButton.SetDriveMode(GpioPinDriveMode.InputPullUp);
            rightButton.ValueChangedEdge = GpioPinEdge.FallingEdge | GpioPinEdge.RisingEdge;


            ////Vibrating Motor
            //var motor = GpioController.GetDefault().OpenPin(SC13048.GpioPin.PA9);
            //motor.SetDriveMode(GpioPinDriveMode.Output);
            //motor.Write(GpioPinValue.Low);

            //Rocker
            var adc1 = AdcController.FromName(SC13048.Adc.Controller1.Id);
            var rockerX = adc1.OpenChannel(SC13048.Adc.Controller1.PA0);

            var adc2 = AdcController.FromName(SC13048.Adc.Controller1.Id);
            var rockerY = adc1.OpenChannel(SC13048.Adc.Controller1.PA1);

            int[] frame1 = new int[] { 1,0,0,1,1,1,1,1,0,
                                       0,1,1,1,1,1,1,1,1,
                                       1,0,0,0,0,1,0,0,0,
                                       1,1,1,1,1,1,1,1,1,
                                       0,0,1,0,0,0,0,0,1
            };
            int[] frame2 = new int[] { 1,0,0,1,1,1,1,1,0,
                                       0,1,1,1,1,1,1,1,1,
                                       1,0,1,0,0,1,1,0,0,
                                       1,1,1,1,1,1,1,1,1,
                                       0,0,1,0,0,0,0,0,1
            };

            int[] bulletImage = new int[] {1,0,
                                           0,1,
                                           1,0,
                                           0,1
            };


            int[] playerImage = new int[] {0,0,0,1,1,1,1,0,0,0,0,
                                           0,0,0,1,1,1,1,0,0,0,0,
                                           1,1,1,1,1,1,1,1,1,1,1,
                                           1,1,1,1,1,1,1,1,1,1,1
            };

            bullet.X = 150;
            bullet.Y = 150;
            bullet.Width = 2;
            bullet.Height = 4;
            bullet.Image = bulletImage;


            enemy.X = 10;
            enemy.Y = 10;
            enemy.Width = 9;
            enemy.Height = 5;
            enemy.Image = frame1;

            enemy2.X = 20;
            enemy2.Y = 10;
            enemy2.Width = 9;
            enemy2.Height = 5;
            enemy2.Image = frame2;

            enemy3.X = 30;
            enemy3.Y = 10;
            enemy3.Width = 9;
            enemy3.Height = 5;
            enemy3.Image = frame1;



            player.X = 75;
            player.Y = 60;
            player.Width = 11;
            player.Height = 4;
            player.Image = playerImage;
            var pCenter = player.Center();

            buzzer.Controller.SetDesiredFrequency(4000);
            buzzer.SetActiveDutyCyclePercentage(0.5);

            while (true) {
                basicGfx.Clear();
                basicGfx.DrawLine(white, 103, 0, 103, 64);

                for (int i = 0; i < lives; i++) {
                    basicGfx.DrawTinyCharacter('^', white, 110, i * 10);//BrainPad.Display.DrawPicture(110, i * 10, Ship);

                }

                basicGfx.DrawTinyString(score.ToString(), white, 110, 55);

                // Process the bullets
                if (BulletIsOut) {

                    //BrainPad.Display.ClearPart(XBullet, YBullet, Bullet.Width, Bullet.Height);
                    bullet.Y -= 12;

                    if (bullet.Y < 0) {
                        BulletIsOut = false;
                        buzzer.Stop();
                    }
                    else {
                        buzzer.Start();
                        DrawSprite(bullet);
                        buzzer.Controller.SetDesiredFrequency(((64 - bullet.Y) + 50) * 50);
                        //buzzer.Controller.SetDesiredFrequency(3000 - bullet.Y * 30);
                        Thread.Sleep(10);
                    }


                    if (bullet.X >= enemy.X - 3 && bullet.X <= enemy.X + enemy.Width + 3 &&
                        bullet.Y >= enemy.Y - 3 && bullet.Y <= enemy.Y + enemy.Height + 3) {
                        score += 10;
                        enemyAlive = false;
                    }
                    if (bullet.X >= enemy2.X - 3 && bullet.X <= enemy2.X + enemy2.Width + 3 &&
                       bullet.Y >= enemy2.Y - 3 && bullet.Y <= enemy2.Y + enemy2.Height + 3) {
                        score += 10;
                        enemyAlive2 = false;
                    }

                    if (bullet.X >= enemy3.X - 3 && bullet.X <= enemy3.X + enemy3.Width + 3 &&
                      bullet.Y >= enemy3.Y - 3 && bullet.Y <= enemy3.Y + enemy3.Height + 3) {
                        score += 10;
                        enemyAlive3 = false;
                    }
                    if (enemyAlive == false && enemyAlive2 == false && enemyAlive3 == false) {

                        enemyAlive = true;
                        enemyAlive2 = true;
                        enemyAlive3 = true;
                        enemy.X = 10;
                        enemy.Y = 10;

                        enemy2.X = 20;
                        enemy2.Y = 10;

                        enemy3.X = 30;
                        enemy3.Y = 10;

                    }


                }
                else {


                    if (upButton.Read() == GpioPinValue.Low) {
                        bullet.Y = 64;//// - Ship.Height - Bullet.Height;

                        bullet.X = player.X + 5;

                        BulletIsOut = true;
                    }
                }

                if (enemyAlive) DrawSprite(enemy);
                if (enemyAlive2) DrawSprite(enemy2);
                if (enemyAlive3) DrawSprite(enemy3);
                DrawSprite(player);

                if (rockerX.ReadRatio() < 0.1) player.X += 5;
                if (rockerX.ReadRatio() > 0.9) player.X -= 5;


                if (player.X < 0) player.X = 0;
                if (player.X > 90) player.X = 90;

                enemy.X += enemySpeed1;
                enemy2.X += enemySpeed2;
                enemy3.X += enemySpeed3;


                if (enemy.X < 5 || enemy.X > 85) {
                    enemySpeed1 *= -1;
                    enemy.Y += 5;
                }
                if (enemy2.X < 5 || enemy2.X > 85) {
                    enemySpeed2 *= -1;
                    enemy2.Y += 5;
                }
                if (enemy3.X < 5 || enemy3.X > 85) {
                    enemySpeed3 *= -1;
                    enemy3.Y += 5;
                }

                if (enemy.Y > 64 || enemy2.Y > 64 || enemy3.Y > 64) {
                    enemy.X = 10;
                    enemy.Y = 10;

                    enemy2.X = 20;
                    enemy2.Y = 10;

                    enemy3.X = 30;
                    enemy3.Y = 10;
                }

                lcd.DrawBufferNative(basicGfx.Buffer);
                Thread.Sleep(5);

            }
        }
        static void DrawSprite(Sprite sprite) {
            var index = 0;
            while (index <= sprite.Image.Length - 1) {
                for (int y = 0; y < sprite.Height; y++) {
                    for (int x = 0; x <= sprite.Width - 1; x++) {
                        if (sprite.Image[index] == 1) {
                            basicGfx.SetPixel(sprite.X + x, sprite.Y + row, white);
                        }
                        else {
                            basicGfx.SetPixel(sprite.X + x, sprite.Y + row, black);
                        }
                        index++;
                    }
                    row++;
                }
                row = 0;
            }

        }
    }
}

