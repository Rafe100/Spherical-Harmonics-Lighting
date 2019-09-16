using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SHLighting : MonoBehaviour
{
    public Cubemap cubeMap;
    public bool SHReadFromFile;
    int degree = 2;
    Dictionary<int, int> faceCalculate = new Dictionary<int, int>();

    private void OnEnable() {
        RenderSettings.skybox.SetTexture("_Tex", cubeMap);
        if (degree > 2) {
            Debug.Log("the degree need to less than 2");
            return;
        }
        int n = (degree + 1) * (degree + 1);
        Vector4[] coefs = new Vector4[n];
       
        if (SHReadFromFile) {
            TextAsset ta = Resources.Load<TextAsset>("SphericalHarmonic");
            var coefArray =   JsonUtility.FromJson<double[]>(ta.ToString());
            Debug.Log("the SphericalHarmonic info is :" + coefArray.Length);
            for (int i = 0; i < coefArray.Length; i++) {
                var coef = new Vector4();
                coef.x = (float)coefArray[i++];
                coef.y = (float)coefArray[i++];
                coef.z = (float)coefArray[i];
                coef.w = 0f;
                coefs[i / 3] = coef;
            }
        } else {
            int sampleNum = 10000;
            faceCalculate.Add(0, 0);
            faceCalculate.Add(1, 0);
            faceCalculate.Add(2, 0);
            faceCalculate.Add(3, 0);
            faceCalculate.Add(4, 0);
            faceCalculate.Add(5, 0);
            for (int i = 0; i < sampleNum; i++) {
                var p = RandomCubePos();
                var h = HarmonicsBasis(p);
                var c = GetCubeColor(p);
                for (int t = 0; t < n; t++) {
                    coefs[t] = coefs[t] + h[t] * c;
                }
            }
            for (int t = 0; t < n; t++) {
                coefs[t] = 4.0f * Mathf.PI * coefs[t] / (sampleNum * 1.0f);
            }
        }
        var mr = transform.GetComponent<MeshRenderer>();
        if (mr == null) {
            Debug.Log("the gameObject need meshRendere component");
            return;
        }
        var mat = mr.material;
        for (int i = 0; i < n; ++i) {
            mat.SetVector("c" + i.ToString(), coefs[i]);
        }
    }

    void SaveSphericalHarmonic(List<Vector4> coefs) {
        List<double> coef = new List<double>();
        foreach (var x in coefs) {
            coef.Add((double)x.x);
            coef.Add((double)x.y);
            coef.Add((double)x.z);
            coef.Add((double)x.w);
        }
        string str = JsonUtility.ToJson(coef);
        Debug.Log("Save  info to text:" + str);
        var txtFile = File.Open(Application.dataPath + "/SphericalHarmonic.txt", FileMode.Create);
        var txtBytes = System.Text.Encoding.Default.GetBytes(str);
        txtFile.Write(txtBytes, 0, txtBytes.Length);
        txtFile.Close();
        Debug.Log("save success !");
    }

    Vector3 RandomCubePos() {
        Vector3 pos = Random.onUnitSphere;
        return pos;
    }

    Vector4 GetCubeColor(Vector3 pos) {

        Color col = new Color();
        //可以使用正态分布

        float xabs = pos.x;
        float yabs = pos.y;
        float zabs = pos.z;
        int faceIndex = -1;
        Vector2 uv = new Vector2();
        if (xabs >= yabs && xabs >= zabs) {
            //x
            faceIndex = pos.x > 0 ? 0 : 1;
            uv.x = pos.y / xabs;
            uv.y = pos.z / xabs;
        } else if (yabs >= xabs && yabs >= zabs) {
            //y 
            faceIndex = pos.y > 0 ? 2 : 3;
            uv.x = pos.x / yabs;
            uv.y = pos.z / yabs;
        } else {
            //z
            faceIndex = pos.z > 0 ? 4 : 5;
            uv.x = pos.x / zabs;
            uv.y = pos.y / zabs;
        }
        //[0,1.0]
        uv.x = (uv.x + 1.0f) / 2.0f;
        uv.y = (uv.y + 1.0f) / 2.0f;
        int w = cubeMap.width - 1;
        int x = (int)(w * uv.x);
        int y = (int)(w * uv.y);
        //Debug.Log("random face:" + faceIndex.ToString());
        if (faceCalculate.ContainsKey(faceIndex)) {
            faceCalculate[faceIndex]++;
        }
        col = cubeMap.GetPixel((CubemapFace)faceIndex, x, y);
        Vector4 colVec4 = new Vector4(col.r, col.g, col.b, col.a);
        return colVec4;
    }


    List<float> HarmonicsBasis(Vector3 pos) {
        int basisCount = (degree + 1) * (degree + 1);
        float[] sh = new float[basisCount];
        for (int i = 0; i < basisCount; i++) {

        }
        Vector3 normal = pos.normalized;
        float x = normal.x;
        float y = normal.y;
        float z = normal.z;

        if (degree >= 0) {
            sh[0] = 1.0f / 2.0f * Mathf.Sqrt(1.0f / Mathf.PI);
        }
        if (degree >= 1) {
            sh[1] = Mathf.Sqrt(3.0f / (4.0f * Mathf.PI)) * z;
            sh[2] = Mathf.Sqrt(3.0f / (4.0f * Mathf.PI)) * y;
            sh[3] = Mathf.Sqrt(3.0f / (4.0f * Mathf.PI)) * x;
        }
        if (degree >= 2) {
            sh[4] = 1.0f / 2.0f * Mathf.Sqrt(15.0f / Mathf.PI) * x * z;
            sh[5] = 1.0f / 2.0f * Mathf.Sqrt(15.0f / Mathf.PI) * z * y;
            sh[6] = 1.0f / 4.0f * Mathf.Sqrt(5.0f / Mathf.PI) * (-x * x - z * z + 2 * y * y);
            sh[7] = 1.0f / 2.0f * Mathf.Sqrt(15.0f / Mathf.PI) * y * x;
            sh[8] = 1.0f / 4.0f * Mathf.Sqrt(15.0f / Mathf.PI) * (x * x - z * z);
        }
        if (degree >= 3) {
            sh[9] = 1.0f / 4.0f * Mathf.Sqrt(35.0f / (2.0f * Mathf.PI)) * (3 * x * x - z * z) * z;
            sh[10] = 1.0f / 2.0f * Mathf.Sqrt(105.0f / Mathf.PI) * x * z * y;
            sh[11] = 1.0f / 4.0f * Mathf.Sqrt(21.0f / (2.0f * Mathf.PI)) * z * (4 * y * y - x * x - z * z);
            sh[12] = 1.0f / 4.0f * Mathf.Sqrt(7.0f / Mathf.PI) * y * (2 * y * y - 3 * x * x - 3 * z * z);
            sh[13] = 1.0f / 4.0f * Mathf.Sqrt(21.0f / (2.0f * Mathf.PI)) * x * (4 * y * y - x * x - z * z);
            sh[14] = 1.0f / 4.0f * Mathf.Sqrt(105.0f / Mathf.PI) * (x * x - z * z) * y;
            sh[15] = 1.0f / 4.0f * Mathf.Sqrt(35.0f / (2 * Mathf.PI)) * (x * x - 3 * z * z) * x;
        }
        List<float> shList = new List<float>(sh);
        return shList;

    }


}
