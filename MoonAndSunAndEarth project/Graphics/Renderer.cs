using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Tao.OpenGl;

//include GLM library
using GlmNet;

using System.IO;
using System.Diagnostics;

namespace Graphics
{
    class Renderer
    {
        Shader sh;

        uint sunBufferID, earthBufferID , moonBufferID;

        mat4 earthMM, sunMM, moonMM;
        mat4 ViewMatrix;
        mat4 ProjectionMatrix;

        int ShaderModelMatrixID;
        int ShaderViewMatrixID;
        int ShaderProjectionMatrixID;

        int S_endIndx , E_endIndx, M_endIndx;
        
        List<float> sunP   = new List<float>();
        List<float> earthP = new List<float>();
        List<float> moonP  = new List<float>();
       
        Stopwatch   timer = Stopwatch.StartNew();
        public void Initialize()
        {
            string projectPath = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName;
            sh = new Shader(projectPath + "\\Shaders\\SimpleVertexShader.vertexshader", projectPath + "\\Shaders\\SimpleFragmentShader.fragmentshader");
            Gl.glClearColor(0, 0, 0, 1);

            // ------------SUN
            for (int i = 0; i < 360; i++) 
            {
                double angle = 2 * Math.PI * i / 360;
                double x = (Math.Cos(angle) * 3), y = (Math.Sin(angle)*3);
                sunP.Add((float)x);
                sunP.Add((float)y);
                sunP.Add((float)0.0f);

                sunP.Add((float)1.0);
                sunP.Add((float)1.0);
                sunP.Add((float)0.0);
            }
            S_endIndx = sunP.Count;
            sunBufferID = GPU.GenerateBuffer(sunP.ToArray());
            
            // ------------EARTH
            for (int i = 0; i < 360; i++)
            {
                double angle = 2 * Math.PI * i / 360;
                double x = (Math.Cos(angle) * 2) + 15, y = (Math.Sin(angle) * 2);
                earthP.Add((float)x);
                earthP.Add((float)y);
                earthP.Add((float)0.0f);

                earthP.Add((float)0.0);
                earthP.Add((float)1.0);
                earthP.Add((float)0.0);
            }
            E_endIndx = earthP.Count;
            earthBufferID = GPU.GenerateBuffer(earthP.ToArray());
            
            // ------------MOON
            for (int i = 0; i < 360; i++)
            {
                double angle = 2 * Math.PI * i / 360;
                double x = (Math.Cos(angle) * 1.2) + 9, y = (Math.Sin(angle) * 1.2)-3;
                moonP.Add((float)x);
                moonP.Add((float)y);
                moonP.Add((float)0.0f);

                moonP.Add((float)1.0);
                moonP.Add((float)1.0);
                moonP.Add((float)1.0);
            }
            M_endIndx = moonP.Count;
            moonBufferID = GPU.GenerateBuffer(moonP.ToArray());

            // ------------
            sunMM = new mat4(1);

            ViewMatrix = glm.lookAt( new vec3(0, 0, 50), new vec3(0, 0, 0), new vec3(0, 1, 0) );

            ProjectionMatrix = glm.perspective(45.0f, 4.0f / 3.0f, 0.1f, 100.0f);

            sh.UseShader();

            ShaderModelMatrixID      = Gl.glGetUniformLocation(sh.ID, "modelMatrix");
            ShaderViewMatrixID       = Gl.glGetUniformLocation(sh.ID, "viewMatrix");
            ShaderProjectionMatrixID = Gl.glGetUniformLocation(sh.ID, "projectionMatrix");

            Gl.glUniformMatrix4fv(ShaderModelMatrixID     , 1 , Gl.GL_FALSE, sunMM.to_array());
            Gl.glUniformMatrix4fv(ShaderViewMatrixID      , 1 , Gl.GL_FALSE, ViewMatrix.to_array());
            Gl.glUniformMatrix4fv(ShaderProjectionMatrixID, 1 , Gl.GL_FALSE, ProjectionMatrix.to_array());

            timer.Start();
        }
        public void drawElem(uint curBufferID , mat4 curMM, int curEndIndx)
        {
            Gl.glBindBuffer(Gl.GL_ARRAY_BUFFER, curBufferID);
            Gl.glUniformMatrix4fv(ShaderModelMatrixID, 1, Gl.GL_FALSE, curMM.to_array());

            Gl.glEnableVertexAttribArray(0);
            Gl.glVertexAttribPointer(0, 3, Gl.GL_FLOAT, Gl.GL_FALSE, 6 * sizeof(float), (IntPtr)0);
            Gl.glEnableVertexAttribArray(1);
            Gl.glVertexAttribPointer(1, 3, Gl.GL_FLOAT, Gl.GL_FALSE, 6 * sizeof(float), (IntPtr)(3 * sizeof(float)));

            Gl.glDrawArrays(Gl.GL_TRIANGLE_FAN, 0, curEndIndx);

            Gl.glDisableVertexAttribArray(0);
            Gl.glDisableVertexAttribArray(1);
        }
        public void Draw()
        {
            sh.UseShader();
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT);

            drawElem(sunBufferID  , sunMM   , S_endIndx);
            drawElem(earthBufferID, earthMM , E_endIndx);
            drawElem(moonBufferID , moonMM  , M_endIndx);
        }

        const float rotationSpeed = 0.007f;
        float rotationAngle = 0;

        vec3 trigCenter2 = new vec3(0 , 0, -5);
        vec3 trigCenter  = new vec3(18, 2, -5);
        public void Update()
        {
            timer.Stop();
            var deltaTime = timer.ElapsedMilliseconds / 1000.0f;
            
            List<mat4> transformations = new List<mat4>();
            transformations.Add( glm.rotate(rotationAngle += rotationSpeed * deltaTime, new vec3(0, 0, 1)));

            List<mat4> transformations2 = new List<mat4>();
            transformations2.Add(glm.rotate(rotationAngle += rotationSpeed * deltaTime, new vec3(0, 0, 1)));
            transformations2.Add(glm.translate(new mat4(1), trigCenter));
            transformations2.Add(glm.translate(new mat4(1), trigCenter2));
            transformations2.Add(glm.rotate(rotationAngle += rotationSpeed * deltaTime, new vec3(0, 0, 1)));

            sunMM   = MathHelper.MultiplyMatrices(transformations);
            earthMM = MathHelper.MultiplyMatrices(transformations);
            moonMM  = MathHelper.MultiplyMatrices(transformations2);

            rotationAngle += rotationSpeed;
        }
        public void CleanUp()
        {
            sh.DestroyShader();
        }
    }
}
