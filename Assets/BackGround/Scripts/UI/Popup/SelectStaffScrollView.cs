using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class SelectStaffScrollView : BaseScrollView<SelectStaffScrollViewItem, SelectStaffInfo>
{
    public int unitCount { get; private set; }
    public int GetSelectCount()
    {
        var count = 0;
        foreach (var item in GetItem())
        {
            if (item.isSelect)
                count++;
        }
        unitCount = count;
        return count;
    }

    protected override void InitFirstItem(SelectStaffScrollViewItem _obj)
    {
        base.InitFirstItem(_obj);

        _obj.selectBtn.OnClickAsObservableThrottleFirst().Subscribe(_=>
        {
            unitCount++;
            _obj.Select();
            if (_obj.GetGray)
                _obj.SetNormal();

            itemClickSubject.OnNext(_obj);
        });

        _obj.DeselectBtn.OnClickAsObservableThrottleFirst().Subscribe(_ =>
        {
            unitCount--;
            _obj.DeSelect();

            itemClickSubject.OnNext(_obj);
        });
    }

    public bool SetLock(bool isLock)
    {
        if (isLock)
        {
            foreach (var mit in GetItem())
            {
                mit.SetLock(!mit.isSelect);
            }
        }

        return isLock;
    }
}
