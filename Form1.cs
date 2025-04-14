namespace FaceCheckToolGUI;

using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;
using DlibDotNet;
using DlibDotNet.Extensions;
using System.Windows.Forms;

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();
    }

    private void btnSelectImage_Click(object sender, EventArgs e)
    {
        using var ofd = new OpenFileDialog();
        ofd.Filter = "Image Files|*.jpg;*.png;*.bmp";
        if (ofd.ShowDialog() == DialogResult.OK)
        {
            string path = ofd.FileName;
            pictureBox1.Image = Image.FromFile(path);
            lblResult.Text = CheckFacePlacement(path);
        }
    }

    private void InitializeComponent()
    {
        btnSelectImage = new Button();
        pictureBox1 = new PictureBox();
        lblResult = new System.Windows.Forms.Label();
        ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
        SuspendLayout();
        // 
        // btnSelectImage
        // 
        btnSelectImage.Location = new System.Drawing.Point(233, 307);
        btnSelectImage.Name = "btnSelectImage";
        btnSelectImage.Size = new Size(86, 27);
        btnSelectImage.TabIndex = 0;
        btnSelectImage.Text = "Select Image\r\n";
        btnSelectImage.UseVisualStyleBackColor = true;
        btnSelectImage.Click += btnSelectImage_Click;
        // 
        // pictureBox1
        // 
        pictureBox1.Location = new System.Drawing.Point(29, 23);
        pictureBox1.Name = "pictureBox1";
        pictureBox1.Size = new Size(507, 251);
        pictureBox1.TabIndex = 1;
        pictureBox1.TabStop = false;
        // 
        // lblResult
        // 
        lblResult.AutoSize = true;
        lblResult.Location = new System.Drawing.Point(109, 368);
        lblResult.Name = "lblResult";
        lblResult.Size = new Size(125, 15);
        lblResult.TabIndex = 2;
        lblResult.Text = "Result will appear here";
        // 
        // Form1
        // 
        ClientSize = new Size(566, 435);
        Controls.Add(lblResult);
        Controls.Add(pictureBox1);
        Controls.Add(btnSelectImage);
        Name = "Form1";
        ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
        ResumeLayout(false);
        PerformLayout();

    }

    private string CheckFacePlacement(string imagePath)
    {
        var img = new Image<Bgr, byte>(imagePath);
        var gray = img.Convert<Gray, byte>();
        int imgWidth = img.Width;
        int imgHeight = img.Height;

        // Blur Check
        var laplacian = gray.Laplace(3);
        var stddev = new MCvScalar();
        var mean = new MCvScalar();
        CvInvoke.MeanStdDev(laplacian, ref mean, ref stddev);
        double variance = stddev.V0 * stddev.V0;
        if (variance < 100)
            return "⚠️ Image is blurry.";

        var faceDetector = new CascadeClassifier("C:/Users/PC27/source/repos/FaceCheckTool/FaceCheckToolGUI/haarcascade_frontalface_default.xml");
        var faces = faceDetector.DetectMultiScale(gray, 1.1, 4, Size.Empty);

        if (faces.Length == 0)
            return "❌ No face detected.";
        if (faces.Length > 1)
            return "❌ Multiple faces detected.";

        var face = faces[0];

        float centerX = face.X + face.Width / 2.0f;
        float centerY = face.Y + face.Height / 2.0f;

        if (!(centerX > 0.4 * imgWidth && centerX < 0.6 * imgWidth &&
              centerY > 0.4 * imgHeight && centerY < 0.6 * imgHeight))
            return "⚠️ Face is not centered.";

        using var dlibImg = Dlib.LoadImage<RgbPixel>(imagePath);
        var detector = Dlib.GetFrontalFaceDetector();
        var predictor = ShapePredictor.Deserialize("C:/Users/PC27/source/repos/FaceCheckTool/FaceCheckToolGUI/shape_predictor_68_face_landmarks.dat");

        var dlibFaces = detector.Operator(dlibImg);
        if (dlibFaces.Length == 0) return "❌ No face (Dlib).";

        var shape = predictor.Detect(dlibImg, dlibFaces[0]);
        var leftEye = shape.GetPart(36);
        var rightEye = shape.GetPart(45);

        double dx = rightEye.X - leftEye.X;
        double dy = rightEye.Y - leftEye.Y;
        double angle = Math.Atan2(dy, dx) * (180.0 / Math.PI);

        if (Math.Abs(angle) > 10)
            return $"⚠️ Head tilt detected ({angle:F2}°).";

        return "✅ Face is good: centered, clear, and not tilted.";
    }
    private Button btnSelectImage;
    private PictureBox pictureBox1;
    private System.Windows.Forms.Label lblResult;

    

}
