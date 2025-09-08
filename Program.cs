using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;

namespace test_carwheel_detector
{
    class Program
    {

        static CustomVisionPredictionClient prediction_client;

        static void Main(string[] args)
        {

            try
            {
                // Get Configuration Settings
                IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
                IConfigurationRoot configuration = builder.Build();
                string prediction_endpoint = configuration["PredictionEndpoint"];
                string prediction_key = configuration["PredictionKey"];
                Guid project_id = Guid.Parse(configuration["ProjectID"]);
                string model_name = configuration["ModelName"];

                // Authenticate a client for the prediction API
                prediction_client = new CustomVisionPredictionClient(new Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.ApiKeyServiceClientCredentials(prediction_key))
                {
                    Endpoint = prediction_endpoint
                };

                // Load the image and prepare for drawing

                //string relative_path_car = Path.Combine("images\\TestingSet", "car_test_1.jpg");
                string relative_path_car = Path.Combine("images\\TestingSet", "audi1.png");
                string relative_path_mag = Path.Combine("images\\mags", "mag1.png");

                // Full path using base directory
                string image_file = Path.Combine(AppContext.BaseDirectory, relative_path_car);
                string mag_file = Path.Combine(AppContext.BaseDirectory, relative_path_mag);


                var image = Image.Load<Rgba32>(image_file);
                int h = image.Height;
                int w = image.Width;
                /*
                Image image = Image.FromFile(image_file);

                Graphics graphics = Graphics.FromImage(image);
                Pen pen = new Pen(Color.Magenta, 3);
                Font font = new Font("Arial", 16);
                SolidBrush brush = new SolidBrush(Color.Black);
                */

                using (var image_data = File.OpenRead(image_file))
                {
                    // Make a prediction against the new project
                    Console.WriteLine("Detecting car wheels in " + image_file);
                    var result = prediction_client.DetectImage(project_id, model_name, image_data);

                    // Loop over each prediction
                    foreach (var prediction in result.Predictions)
                    {
                        // Get each prediction with a probability > 50%
                        if (prediction.Probability > 0.5)
                        {
                            // The bounding box sizes are proportional - convert to absolute
                            int left = Convert.ToInt32(prediction.BoundingBox.Left * w);
                            int top = Convert.ToInt32(prediction.BoundingBox.Top * h);
                            int height = Convert.ToInt32(prediction.BoundingBox.Height * h);
                            int width = Convert.ToInt32(prediction.BoundingBox.Width * w);

                           
                           
                            Rectangle rect = new Rectangle(left, top, width, height);
                            MagPlacement.ReplaceCarMag (image_file, rect , mag_file); 
                            

                             // Draw the bounding box
                            //graphics.DrawRectangle(pen, rect);
                            // Annotate with the predicted label
                            //graphics.DrawString(prediction.TagName, font, brush, left, top);

                        }
                    }
                }
                // Save the annotated image
                String output_file = "output.jpg";
                image.Save(output_file);
                Console.WriteLine("Results saved in " + output_file);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

        }
        



    }
}
