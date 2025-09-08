# Car Wheel (Mag) Replacement with Azure Custom Vision + EmguCV

This project demonstrates how to **detect car wheels using Azure Custom Vision** and then **replace them with a different wheel (mag) image** using **EmguCV** for perspective warping and blending.

---

## 📌 Features
- Uses **Azure Custom Vision** to detect car wheels in an image.
- Automatically calculates wheel bounding boxes.
- Removes the original wheel using **OpenCV inpainting**.
- Warps and overlays a replacement mag image using **perspective transform**.
- Blends the new mag realistically with alpha transparency.

---

## 🛠️ Requirements

- [.NET 7 SDK or later](https://dotnet.microsoft.com/)
- [Emgu.CV](http://www.emgu.com/wiki/index.php/Main_Page) (OpenCV for .NET)
- [ImageSharp](https://github.com/SixLabors/ImageSharp)
- Azure Cognitive Services:
  - Custom Vision **Prediction Endpoint**
  - Custom Vision **Prediction Key**
  - Custom Vision **Project ID** and **Model Name**

---

## 📦 Setup

1. **Clone or copy this project.**

2. **Install dependencies:**
   ```bash
   dotnet add package Emgu.CV
   dotnet add package Emgu.CV.Bitmap
   dotnet add package Emgu.CV.runtime.windows
   dotnet add package SixLabors.ImageSharp
   dotnet add package Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction
   ```

3. **Configure Azure Custom Vision:**

   Create an `appsettings.json` file in the project root with the following:

   ```json
   {
     "PredictionEndpoint": "https://<your-endpoint>.cognitiveservices.azure.com/",
     "PredictionKey": "<your-prediction-key>",
     "ProjectID": "<your-project-id-guid>",
     "ModelName": "<your-model-name>"
   }
   ```

4. **Add input images:**
   - Place car test images under `images/TestingSet/`
   - Place mag (wheel) images under `images/mags/`

   Example:
   ```
   images/
   ├── TestingSet/
   │   └── audi1.png
   └── mags/
       └── mag1.png
   ```

---

## ▶️ Running the Program

Run the console app:

```bash
dotnet run
```

What happens:
1. The program loads a car image.
2. Sends it to Azure Custom Vision to detect wheels.
3. For each detected wheel:
   - Removes the original wheel area using **inpainting**.
   - Warps and overlays the replacement mag.
4. Saves the final result as:

- `car_with_mag.png` → processed image with the replaced wheel  
- `output.jpg` → image annotated and saved by ImageSharp

---

## 📂 Project Structure

- `Program.cs` → Main entry point. Loads config, runs Azure Custom Vision detection, and calls mag replacement.  
- `MagPlacement.cs` → Handles wheel removal, perspective transform, and blending the mag onto the car.  
- `RectangleExtensions.cs` → Converts bounding box rectangles into corner points for transforms.  
- `appsettings.json` → Stores Azure Custom Vision credentials and project info.  

---

## 🧪 Example Output

Input:  
🚗 Car image (`audi1.png`)  
🛞 Mag image (`mag1.png`)  

Output:  
📸 `car_with_mag.png` with new wheels applied.

---

## ⚠️ Notes

- Only **front-left wheel** is demonstrated, but the approach can be extended to multiple wheels.
- Ensure mag images are **transparent PNGs**.
- If running on Linux/macOS, `System.Drawing.Common` is not supported. Stick with ImageSharp or SkiaSharp for any drawing needs.
- Perspective warping quality depends on bounding box accuracy from Custom Vision.

