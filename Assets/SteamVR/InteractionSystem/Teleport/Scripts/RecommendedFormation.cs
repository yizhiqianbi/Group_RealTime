using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Valve.VR;
public class RecommendedFormation : MirrorBasicFormation
{
    private Transform exhibition;
    //private List<Transform> exhibitions;
    //public float secondFloorHeight = 3.42f;
    private float distance = 0.5f;
    private float angle = 0;
    Stopwatch sw;
    public override void UpdateFormation()
    {
        if (exhibition) exhibition.gameObject.GetComponent<IdentificationRange>().ShowUnSelected();

        UpdateOpaque();//����͸���ȣ�����ת͸����������͸��

        if (!mirrorDestinationFormation.isTeleporting) return;

        exhibition = mirrorDestinationFormation.IsExhibition();
        if (exhibition) exhibition.gameObject.GetComponent<IdentificationRange>().ShowSelected();
            



        if (!mirrorDestinationFormation.recommendedFormationMode)
        {
            return;
        }
        bool rotateLeft = rotateLeftAction.GetState(SteamVR_Input_Sources.LeftHand);
        bool rotateRight = rotateRightAction.GetState(SteamVR_Input_Sources.LeftHand);
        if (rotateLeft)
        {
            angle += 360f / 5 / 2000 * 100;
        }
        else if (rotateRight)
        {
            angle -= 360f / 5 / 2000 * 100;
        }

        //���ݵ�ǰ��transform���£�1.Ŀ���ж� 2.��չƷ-�� 3.չƷ-��Ԥ���������
        if (exhibition)
        {
            ShowFormation_E();
            if (mirrorDestinationFormation.showArrow) UpdateArrow();//���ӻ���ƫת�Ƕ�
        }
        else
        {
            ShowFormation_NE();
        }
    }

    void UpdateOpaque()
    {
        RenderingMode renderingMode;
        if (mirrorDestinationFormation.isTeleporting) { renderingMode = RenderingMode.Transparent; }
        else { renderingMode = RenderingMode.Opaque; }
        for (int i = 0; i < index2connectionMap.Count; i++)//��ʵ�û�����Ҫ�ͻ��˷������
        {
            NetworkConnection targetConnection = index2connectionMap[i];
            targetConnection.identity.gameObject.GetComponent<Opaque>().renderingModeValue = (int)renderingMode;
        }
        for (int i = index2connectionMap.Count; i < preAvatars.Count; i++)//ģ���û����������˸��¼���
        {
            GameObject simulator = index2simulatorMap[i];
            simulator.GetComponent<Opaque>().renderingModeValue = (int)renderingMode;
        }
    }

    void UpdateArrow()
    {
        for (int i = 0; i < preAvatars.Count; i++)
        {
            GameObject preAvatar = preAvatars[i];
            GameObject visitor = null;
            if (i < index2connectionMap.Count)//��ʵ�û�
            {
                visitor = index2connectionMap[i].identity.gameObject.transform.Find("Avatar").gameObject;
            }
            else
            {
                visitor = index2simulatorMap[i];
            }
            Vector3 preForward = visitor.transform.forward;
            preAvatar.transform.Find("Head/PreArrow").GetComponent<ArrowScript>().SetArrowForward(preForward);
        }
    }


    /*    public Transform IsExhibition()
        {
            foreach(Transform e in exhibitions)
            {
                if((e.position.y < secondFloorHeight && transform.position.y < secondFloorHeight)||
                    e.position.y > secondFloorHeight && transform.position.y > secondFloorHeight)//��ͬһ��
                {
                    float ex = e.position.x;
                    float ez = e.position.z;
                    float x = transform.position.x;
                    float z = transform.position.z;
                    float r = e.gameObject.GetComponent<IdentificationRange>().range;
                    if (Mathf.Pow((ex - x), 2) + Mathf.Pow((ez - z), 2) < Mathf.Pow(r, 2))//��ʶ��������
                    {
                        return e;
                    }
                }
            }
            return null;
        }*/

    void ShowFormation_E()
    {
        sw.Start();
        //1.չʾ��Χ

        //2.չʾ����
        //��Ҫȷ��������ƫ�ã�(5,15,20)
        List<Tuple<Transform, Preference>> avatars = new List<Tuple<Transform, Preference>>();
        for(int i = 0; i < preAvatars.Count; i++)
        {
            if (i < index2connectionMap.Count)
            {
                Transform transform = index2connectionMap[i].identity.gameObject.transform.Find("Avatar");
                Preference preference = index2connectionMap[i].identity.gameObject.GetComponent<PreferenceScript>().GetPreference();
                Tuple<Transform, Preference> avatar = new Tuple<Transform, Preference>(transform, preference);
                avatars.Add(avatar);
            }
            else
            {
                Transform transform = index2simulatorMap[i].transform;
                Preference preference = Preference.Normal;
                /*switch (i)
                {
                    case 0:
                        preference = Preference.Far;
                        break;
                    case 3://purple
                        preference = Preference.Close;
                        break;
                    case 4://red
                        preference = Preference.Close;
                        break;
                }*/
                Tuple<Transform, Preference> avatar = new Tuple<Transform, Preference>(transform, preference);
                avatars.Add(avatar);
            }
        }

        List<Vector3> recommendedPositions = exhibition.gameObject.GetComponent<GetFormation>().GetRecommendedFormation(avatars);
        if (recommendedPositions != null)
        {
            for (int i = 0; i < preAvatars.Count; i++)
            {
                preAvatars[i].transform.position = recommendedPositions[i];
                Vector3 forward = exhibition.position - preAvatars[i].transform.position;
                preAvatars[i].transform.forward = new Vector3(forward.x, 0, forward.z);
            }
            //����ģ���û��ĳ��򣺿���Ԥ������
            if (mirrorDestinationFormation.isTeleporting && mirrorDestinationFormation.lookAtGuide)
            {
                for (int i = index2connectionMap.Count; i < preAvatars.Count; i++)
                {
                    GameObject simulator = index2simulatorMap[i];
                    Vector3 forward = preAvatars[0].transform.position - simulator.transform.position;
                    simulator.transform.forward = new Vector3(forward.x, 0, forward.z);
                }
            }
            sw.Stop();
            //UnityEngine.Debug.Log("ʱ��: " + sw.ElapsedMilliseconds + " ����");
        }
    }

    void ShowFormation_NE()
    {
        int size = 3;
        while (true)
        {
            if (size > 50)
            {
                UnityEngine.Debug.Log("��ѭ��!����û�п���ת��");
                return;
            }
            if (TryFill(size))
            {
                break;
            }
            size++;
        }
    }

    bool TryFill(int size)
    {
        int index = 0;
        int mem_num = preAvatars.Count;
        for(int i = 0; i < size; i++)
        {
            for(int j = 0; j < size; j++)
            {
                Vector3 offsetToTransform = new Vector3(j * distance, 0, i * distance);
                Vector3 position = transform.position + offsetToTransform;
                if (IsValidPoint(position))
                {
                    preAvatars[index].transform.position = position + new Vector3(0, offset, 0);
                    preAvatars[index].transform.forward = new Vector3(Mathf.Cos(angle / 180 * Mathf.PI), 0, Mathf.Sin(angle / 180 * Mathf.PI));
                    index++;
                    if(index == mem_num)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    bool IsValidPoint(Vector3 position)
    {
        //������0.1������ײ��
        Collider[] colliders = Physics.OverlapSphere(position - new Vector3(0, 0.1f, 0), 0.05f);
        if (colliders.Length != 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public override void CheckValidation()
    {
        //�ո��Ǽ���
    }

    public new void Awake()
    {
        base.Awake();
        formationType = "auto";
        sw = new Stopwatch();
    }

}
