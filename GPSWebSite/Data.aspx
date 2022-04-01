<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="LastData.aspx.cs" Inherits="GPSTrackerService.Data" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title>GPS Tracker</title>
    <script src="http://api-maps.yandex.ru/2.1/?load=package.full&lang=ru-RU" type="text/javascript"></script>
    <script type="text/javascript" src="http://js.static.yandex.net/jquery/1.3.2/_jquery.js"></script>
    <script type="text/javascript" src="https://momentjs.com/downloads/moment.js"></script>
    <script type="text/javascript">
        var map;
        ymaps.ready(init);


        function refresh() {
            map.destroy();
            initMap();
        }

        function formatDate(date) {

            var day = date.getDate().toString();
            if (day.length == 1)
                day = '0' + day;
            var monthIndex = (date.getMonth() + 1).toString();
            if (monthIndex.length == 1)
                monthIndex = '0' + monthIndex;
            var year = date.getFullYear();

            return year + '-' + monthIndex + '-' + day;
        }

        function showLast() {
            var x = document.getElementById('historyDiv');
            x.style.display = 'none';
            x = document.getElementById('lastDiv');
            x.style.display = 'inline-block';
        }
        function showHistory() {

            var x = document.getElementById('historyDiv');
            x.style.display = 'inline-block';
            x = document.getElementById('lastDiv');
            x.style.display = 'none';
        }

        function init() {

            var endday = new Date();
            var startday = new Date();
            startday.setTime(endday.getTime() - 24 * 60 * 60 * 1000);
            document.getElementById("historyTo").value = formatDate(endday);
            document.getElementById("historyFrom").value = formatDate(startday);

            initMap();
        }

        function initMap() {


            $.post('res/Handler.ashx', {
                fromDB: document.getElementById('DB').checked,
                speedColor: document.getElementById('SpeedColor').checked,
                count: document.getElementById('MaxPoints').value,
                lastHours: document.getElementById('LastHours').value,
                historyTo: document.getElementById('historyTo').value,
                historyFrom: document.getElementById('historyFrom').value,
            }, function (response) {
                eval(response);

                if (lastPoint == null) {
                    map = new ymaps.Map('YMapsID', {
                        center: [59.98458667, 30.24550500],
                        zoom: 8
                    });
                    return;
                }

                map = new ymaps.Map('YMapsID', {
                    center: lastPoint[0],
                    zoom: 12
                });

                var itemsCollection = new ymaps.GeoObjectCollection();


                var MyIconContentLayout = ymaps.templateLayoutFactory.createClass(
                    '<div style="color: #FFFFFF; font-weight: bold; font-size:8pt;">$[properties.iconContent]</div>'
                );

                for (var i = 0; i < data.Stops.length; i++) {
                    itemsCollection.add(new ymaps.Placemark([data.Stops[i].Latitude, data.Stops[i].Longitude], {
                        hintContent: data.Stops[i].Description,
                        balloonContent: data.Stops[i].Description,
                        iconContent: data.Stops[i].Market
                    }, {
                        iconLayout: 'default#imageWithContent',
                        iconImageHref: 'res/stop.png',
                        iconImageSize: [48, 48],
                        iconImageOffset: [-24, -48],
                        iconContentOffset: [19, 6],
                        iconContentLayout: MyIconContentLayout
                    }));
                }

                for (var i = 0; i < data.Starts.length; i++) {
                    itemsCollection.add(new ymaps.Placemark([data.Starts[i].Latitude, data.Starts[i].Longitude], {
                        hintContent: data.Starts[i].Description,
                        balloonContent: data.Starts[i].Description,
                        iconContent: data.Stops[i].Market
                    }, {
                        iconLayout: 'default#imageWithContent',
                        iconImageHref: 'res/start.png',
                        iconImageSize: [32, 32],
                        iconImageOffset: [-16, -32],
                        iconContentOffset: [11, 2],
                        iconContentLayout: MyIconContentLayout
                    }));
                }

                for (var i = 0; i < data.Arrows.length; i++) {
                    itemsCollection.add(new ymaps.Polyline(
                        [
                            [data.Arrows[i].LatitudeStart, data.Arrows[i].LongitudeStart],
                            [data.Arrows[i].Latitude, data.Arrows[i].Longitude]
                        ], {
                            hintContent: data.Arrows[i].Description,
                            balloonContent: data.Arrows[i].Description
                        },
                        {
                            strokeColor: data.Arrows[i].Color,
                            strokeWidth: 6
                        }));
                }

                itemsCollection.add(new ymaps.Circle(
                    [lastPoint[0], 0],
                    { hintContent: lastPoint[1] },
                    { fillColor: lastPoint[2] + "00", strokeColor: lastPoint[2], strokeWidth: 20 }
                ));

                map.geoObjects.add(itemsCollection);

            });
        }
    </script>
    <style>
        html, body, #YMapsID {
            width: 100%;
            height: 95%;
            padding: 0;
            margin: 0;
        }

        .margins {
            margin: 10px;
            display: inline-block
        }
    </style>
</head>

<body>
    <div class="margins">
        <input type="button" value="Обновить" onclick="refresh()" />
        <input type="radio" name="gr" id="Last" checked="checked" onclick="showLast()" />Сейчас
        <input type="radio" name="gr" id="DB" onclick="showHistory()" />История
    </div>
    <div class="margins" >
        <div id="lastDiv" style="display: inline-block">
            <label>За последние(ч):</label>
            <input id="LastHours" value="12" />
        </div>

        <div id="historyDiv" style="display: none">
            с <input type="date" id="historyFrom" />
             по <input type="date" id="historyTo" />
        </div>
        <label>  Количество точек:</label>
        <input id="MaxPoints" value="500"/>
        <input type="checkbox" id="SpeedColor" checked="checked" />
    </div>
    <div id="YMapsID" />
</body>
</html>
