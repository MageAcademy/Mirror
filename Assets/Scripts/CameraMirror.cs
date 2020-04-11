using UnityEngine;

namespace com.PROS.Library
{
    [ExecuteInEditMode]
    public class CameraMirror : MonoBehaviour
    {
        public Camera cameraMirror;
        public Camera mainCamera;
        public Transform planeMirror;

        private Vector3 _mCameraMirrorPosition;
        private Vector3 _mPlaneLeftBottom;
        private Vector3 _mPlaneLeftTop;
        private Vector3 _mPlaneRightBottom;
        private Vector3[] _mCameraToPlane = new Vector3[3];
        private Vector3 _mPlaneXAxis;
        private Vector3 _mPlaneYAxis;
        private Vector3 _mPlaneZAxis;
        private float _mFarClipPlane;
        private float _mNearClipPlane;
        private float _mBottom;
        private float _mLeft;
        private float _mRight;
        private float _mTop;
        private Matrix4x4 _mCameraProjectionMatrix;
        private Matrix4x4 _mCameraRotationMatrix;
        private Matrix4x4 _mCameraTranslationMatrix;

        private void Update()
        {
            //找到镜面相机的位置
            _mCameraMirrorPosition = planeMirror.InverseTransformPoint(mainCamera.transform.position);
            if (_mCameraMirrorPosition.y < 0.1f)
            {
                return;
            }

            _mCameraMirrorPosition.y *= -1f;
            _mCameraMirrorPosition = planeMirror.TransformPoint(_mCameraMirrorPosition);
            cameraMirror.transform.position = _mCameraMirrorPosition;
            //找到Plane相对于镜面相机的左下、左上、右上的三个顶点的位置
            _mPlaneLeftBottom = planeMirror.TransformPoint(new Vector3(-5f, 0f, 5f));
            _mPlaneLeftTop = planeMirror.TransformPoint(new Vector3(-5f, 0f, -5f));
            _mPlaneRightBottom = planeMirror.TransformPoint(new Vector3(5f, 0f, 5f));
            //求出Plane本地坐标系的三个坐标轴向量
            _mPlaneXAxis = _mPlaneRightBottom - _mPlaneLeftBottom;
            _mPlaneYAxis = -planeMirror.up;
            _mPlaneZAxis = _mPlaneLeftBottom - _mPlaneLeftTop;
            //求出镜面相机到这三个顶点的向量
            _mCameraToPlane[0] = _mPlaneLeftBottom - _mCameraMirrorPosition;
            _mCameraToPlane[1] = _mPlaneLeftTop - _mCameraMirrorPosition;
            _mCameraToPlane[2] = _mPlaneRightBottom - _mCameraMirrorPosition;
            //求出公式所需的远近截面距离
            _mFarClipPlane = cameraMirror.farClipPlane;
            _mNearClipPlane = Vector3.Dot(_mCameraToPlane[0], -_mPlaneYAxis);
            cameraMirror.nearClipPlane = _mNearClipPlane;
            //求出公式所需的上下左右
            _mPlaneXAxis.Normalize();
            _mPlaneZAxis.Normalize();
            _mBottom = Vector3.Dot(_mCameraToPlane[0], _mPlaneZAxis);
            _mLeft = Vector3.Dot(_mCameraToPlane[0], _mPlaneXAxis);
            _mRight = Vector3.Dot(_mCameraToPlane[2], _mPlaneXAxis);
            _mTop = Vector3.Dot(_mCameraToPlane[1], _mPlaneZAxis);
            //代入公式，求出镜面相机的投影矩阵
            if (Mathf.Abs(_mLeft - _mRight) < 0.01f || Mathf.Abs(_mBottom - _mTop) < 0.01f ||
                Mathf.Abs(_mFarClipPlane - _mNearClipPlane) < 0.01f)
            {
                return;
            }

            _mCameraProjectionMatrix.m00 = _mNearClipPlane / (_mRight - _mLeft) * 2f;
            _mCameraProjectionMatrix.m01 = 0f;
            _mCameraProjectionMatrix.m02 = (_mLeft + _mRight) / (_mRight - _mLeft);
            _mCameraProjectionMatrix.m03 = 0f;
            _mCameraProjectionMatrix.m10 = 0f;
            _mCameraProjectionMatrix.m11 = _mNearClipPlane / (_mTop - _mBottom) * 2f;
            _mCameraProjectionMatrix.m12 = (_mBottom + _mTop) / (_mTop - _mBottom);
            _mCameraProjectionMatrix.m13 = 0f;
            _mCameraProjectionMatrix.m20 = 0f;
            _mCameraProjectionMatrix.m21 = 0f;
            _mCameraProjectionMatrix.m22 = (_mFarClipPlane + _mNearClipPlane) / (_mNearClipPlane - _mFarClipPlane);
            _mCameraProjectionMatrix.m23 = _mFarClipPlane * _mNearClipPlane / (_mNearClipPlane - _mFarClipPlane) * 2f;
            _mCameraProjectionMatrix.m30 = 0f;
            _mCameraProjectionMatrix.m31 = 0f;
            _mCameraProjectionMatrix.m32 = -1f;
            _mCameraProjectionMatrix.m33 = 0f;
            cameraMirror.projectionMatrix = _mCameraProjectionMatrix;
            //求出镜面相机的缩放、旋转、位移矩阵
            _mCameraRotationMatrix.m00 = _mPlaneXAxis.x;
            _mCameraRotationMatrix.m01 = _mPlaneXAxis.y;
            _mCameraRotationMatrix.m02 = _mPlaneXAxis.z;
            _mCameraRotationMatrix.m03 = 0f;
            _mCameraRotationMatrix.m10 = _mPlaneZAxis.x;
            _mCameraRotationMatrix.m11 = _mPlaneZAxis.y;
            _mCameraRotationMatrix.m12 = _mPlaneZAxis.z;
            _mCameraRotationMatrix.m13 = 0f;
            _mCameraRotationMatrix.m20 = _mPlaneYAxis.x;
            _mCameraRotationMatrix.m21 = _mPlaneYAxis.y;
            _mCameraRotationMatrix.m22 = _mPlaneYAxis.z;
            _mCameraRotationMatrix.m23 = 0f;
            _mCameraRotationMatrix.m30 = 0f;
            _mCameraRotationMatrix.m31 = 0f;
            _mCameraRotationMatrix.m32 = 0f;
            _mCameraRotationMatrix.m33 = 1f;
            _mCameraTranslationMatrix.m00 = 1f;
            _mCameraTranslationMatrix.m01 = 0f;
            _mCameraTranslationMatrix.m02 = 0f;
            _mCameraTranslationMatrix.m03 = -_mCameraMirrorPosition.x;
            _mCameraTranslationMatrix.m10 = 0f;
            _mCameraTranslationMatrix.m11 = 1f;
            _mCameraTranslationMatrix.m12 = 0f;
            _mCameraTranslationMatrix.m13 = -_mCameraMirrorPosition.y;
            _mCameraTranslationMatrix.m20 = 0f;
            _mCameraTranslationMatrix.m21 = 0f;
            _mCameraTranslationMatrix.m22 = 1f;
            _mCameraTranslationMatrix.m23 = -_mCameraMirrorPosition.z;
            _mCameraTranslationMatrix.m30 = 0f;
            _mCameraTranslationMatrix.m31 = 0f;
            _mCameraTranslationMatrix.m32 = 0f;
            _mCameraTranslationMatrix.m33 = 1f;
            cameraMirror.worldToCameraMatrix = _mCameraRotationMatrix * _mCameraTranslationMatrix;
        }
    }
}