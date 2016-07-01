using System;
using System.Collections.Generic;
using System.Text;
using OpenTK; //La matematica

namespace CGUNS
{
    public class Material
    {
        static float gloss = 128.0f;

        //SOME MATERIALS
        public static Material Default = new Material(
        new Vector3(0.5f, 0.5f, 0.5f),
        new Vector3(0.7f, 0.7f, 0.7f),
        new Vector3(0.5f, 0.5f, 0.5f),
        0.07f * gloss);

        public static Material MetalTank = new Material(
        new Vector3(0.3f, 0.3f, 0.3f),
        new Vector3(0.45f, 0.45f, 0.45f),
        new Vector3(1, 1, 1),
        0.5f * gloss);

        public static Material Terrain = new Material(
        new Vector3(0.5f, 0.5f, 0.5f),
        new Vector3(0.5f, 0.5f, 0.5f),
        new Vector3(1f, 1f, 1f),
        0.25f * gloss);

        public static Material Edificio1 = new Material(
        new Vector3(0.6f, 0.6f, 0.6f),
        new Vector3(0.9f, 0.9f, 0.9f),
        new Vector3(0.04f, 0.07f, 0.07f),
        0.078125f * gloss);

        public static Material Edificio2 = new Material(
        new Vector3(0.3f, 0.3f, 0.3f),
        new Vector3(0.5f, 0.5f, 0.5f),
        new Vector3(0.04f, 0.07f, 0.07f),
        0.078125f * gloss);

        public static Material Ruby = new Material(
        new Vector3(0.1745f, 0.01175f, 0.01175f),
        new Vector3(0.61424f, 0.04136f, 0.04136f),
        new Vector3(0.727811f, 0.626959f, 0.626959f),
        0.6f * gloss);

    
        public static Material Brass = new Material(
        new Vector3(0.329412f, 0.223529f, 0.027451f),
        new Vector3(0.780392f, 0.568627f, 0.113725f),
        new Vector3(0.992157f, 0.941176f, 0.807843f),
        0.21794872f * gloss);

        public static Material Bronze = new Material(
        new Vector3(0.3125f, 0.2275f, 0.154f),
        new Vector3(0.714f, 0.4284f, 0.18144f),
        new Vector3(0.5f, 0.4f, 0.1f),
        0.5f * gloss);

        public static Material Chrome = new Material(
        new Vector3(0.25f, 0.25f, 0.25f),
        new Vector3(0.4f, 0.4f, 0.4f),
        new Vector3(0.774597f, 0.774597f, 0.774597f),
        0.6f * gloss);

        public static Material Copper = new Material(
        new Vector3(0.19125f, 0.0735f, 0.0225f),
        new Vector3(0.7038f, 0.27048f, 0.0828f),
        new Vector3(0.256777f, 0.137622f, 0.086014f),
        0.1f * gloss);

        public static Material Gold = new Material(
        new Vector3(0.24725f, 0.1995f, 0.0745f),
        new Vector3(0.75164f, 0.60648f, 0.22648f),
        new Vector3(0.628281f, 0.555802f, 0.366065f),
        0.4f * gloss);

        public static Material Silver = new Material(
        new Vector3(0.19225f, 0.19225f, 0.19225f),
        new Vector3(0.50754f, 0.50754f, 0.50754f),
        new Vector3(0.508273f, 0.508273f, 0.508273f),
        0.4f * gloss);

        public static Material BlackPlastic = new Material(
        new Vector3(0.0f, 0.0f, 0.0f),
        new Vector3(0.01f, 0.01f, 0.01f),
        new Vector3(0.50f, 0.50f, 0.50f),
        .25f * gloss);

        public static Material CyanPlastic = new Material(
        new Vector3(0.0f, 0.1f, 0.06f),
        new Vector3(0.0f, 0.50980392f, 0.50980392f),
        new Vector3(0.50196078f, 0.50196078f, 0.50196078f),
        0.25f * gloss);

        public static Material GreenPlastic = new Material(
        new Vector3(0.0f, 0.0f, 0.0f),
        new Vector3(0.1f, 0.35f, 0.1f),
        new Vector3(0.45f, 0.55f, 0.45f),
        0.25f * gloss);

        public static Material RedPlastic = new Material(
        new Vector3(0.0f, 0.0f, 0.0f),
        new Vector3(0.5f, 0.0f, 0.0f),
        new Vector3(0.7f, 0.6f, 0.6f),
        .25f * gloss);

        public static Material WhitePlastic = new Material(
        new Vector3(0.0f, 0.0f, 0.0f),
        new Vector3(0.55f, 0.55f, 0.55f),
        new Vector3(0.70f, 0.70f, 0.70f),
        .25f * gloss);

        public static Material YellowPlastic = new Material(
        new Vector3(0.0f, 0.0f, 0.0f),
        new Vector3(0.5f, 0.5f, 0.0f),
        new Vector3(0.60f, 0.60f, 0.50f),
        0.25f * gloss);

        public static Material BlackRubber = new Material(
        new Vector3(0.02f, 0.02f, 0.02f),
        new Vector3(0.01f, 0.01f, 0.01f),
        new Vector3(0.4f, 0.4f, 0.4f),
        0.078125f * gloss);

        

        public static Material GreenRubber = new Material(
        new Vector3(0.0f, 0.05f, 0.0f),
        new Vector3(0.4f, 0.5f, 0.4f),
        new Vector3(0.04f, 0.7f, 0.04f),
        0.078125f * gloss);

        public static Material RedRubber = new Material(
        new Vector3(0.05f, 0.0f, 0.0f),
        new Vector3(0.5f, 0.4f, 0.4f),
        new Vector3(0.7f, 0.04f, 0.04f),
        0.078125f * gloss);

        public static Material WhiteRubber = new Material(
        new Vector3(0.05f, 0.05f, 0.05f),
        new Vector3(0.5f, 0.5f, 0.5f),
        new Vector3(0.7f, 0.7f, 0.7f),
        0.078125f * gloss);

        public static Material YellowRubber = new Material(
        new Vector3(0.05f, 0.05f, 0.0f),
        new Vector3(0.5f, 0.5f, 0.4f),
        new Vector3(0.7f, 0.7f, 0.04f),
        0.078125f * gloss);

        Vector3 kambient;
        Vector3 kdiffuse;
        Vector3 kspecular;
        float shininess;

        public Material()
        {
            kambient = new Vector3();
            kdiffuse = new Vector3();
            kspecular = new Vector3();
            shininess = 0.0f;
        }

        public Material(Vector3 kambient, Vector3 kdiffuse, Vector3 kspecular, float shininess)
        {
            this.kambient = kambient;
            this.kdiffuse = kdiffuse;
            this.kspecular = kspecular;
            this.shininess = shininess;
        }

        public Vector3 Kambient
        {
            get
            {
                return kambient;
            }
            set
            {
                this.kambient = value;
            }
        }
        public Vector3 Kdiffuse
        {
            get
            {
                return kdiffuse;
            }
            set
            {
                this.kdiffuse = value;
            }
        }
        public Vector3 Kspecular
        {
            get
            {
                return kspecular;
            }
            set
            {
                this.kspecular = value;
            }
        }
        public float Shininess
        {
            get
            {
                return shininess;
            }
            set
            {
                this.shininess = value;
            }
        }

    }
}
