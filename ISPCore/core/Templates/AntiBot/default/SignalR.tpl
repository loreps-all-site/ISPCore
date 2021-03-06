﻿<!DOCTYPE html>
<html lang='ru'>
<head>
    <title>Защита от ботов</title>
    <meta http-equiv='Content-Type' content='text/html; charset=utf-8'>
    <meta name='referrer' content='no-referrer' />
    <link rel='stylesheet' href='https://cdnjs.cloudflare.com/ajax/libs/font-awesome/4.7.0/css/font-awesome.css'>
    <link rel='stylesheet' href='/statics/antibot.css'>
    <script type='text/javascript' src='/statics/jquery.min.js'></script>
	<script type='text/javascript' src='/statics/signalr-clientES5-1.0.0-alpha2-final.min.js'></script>
</head>

<body class='payment payment-secuses'>
    <div class='text-center payment-box'>
        <div>
            <i id='FaWait' class='fa fa-cogs payment-icon' style='display: none'></i>
            <i id='FaError' class='fa fa-wheelchair-alt payment-icon' style='color: #F04E51'></i>
        </div>
        <h3 class='payment-title'>AntiBot</h3>
        <p class='payment-text' id='InfoText'>У вас должен быть включён JavaScript и Cookie</p>
        <p class='payment-text' id='timeText'>wait <span class='time'>-</span> ms</p>

        <div class='block'>
            <div class='copyright'>
                <div style='margin-bottom: 10px;'>
                    © 2018 <strong>ISPCore</strong>. All rights reserved.
                </div>
                <div>
                    <a href='/'>Главная сайта</a> / <a href='http://core-system.org/' target='_blank'>Core System</a>
                </div>
            </div>
        </div>
    </div>

    <script>
        $('#InfoText').text('Пожалуйста подождите, идет проверка браузера')
        $('#FaWait').show()
        $('#FaError').hide()
    </script>


{isp:JsToTimer}

{isp:JsToBase64}


    <script>
        {isp:JsToRewriteUser}
        else
        {
            setTimeout(function()
            {
	            HubConnection = new signalR.HttpConnection('{isp:CoreApiUrl}/AntiBotHub');
	            Hub = new signalR.HubConnection(HubConnection);

                Hub.on('OnError', function(error) 
                {
                    $('#InfoText').text(error)
                    $('#FaError').show()
                    $('#FaWait').hide()
                    $('#timeText').hide()
                })

	            Hub.on('OnCookie', function(cookie, HourToCookie) 
                {
                    var date = new Date(new Date().getTime() + (60 * 1000) * 60 * HourToCookie);
                    document.cookie = 'isp.ValidCookie='+cookie+'; path=/; expires=' + date.toUTCString();

                    $.post('{isp:CoreApiUrl}/check/cookie', $('#form').serializeArray(), function (data) 
                    {
                        var json = JSON.parse(JSON.stringify(data));
                        if (json.result) {
                            location.reload();
                        }
                        else if (json.msg) {
                            $('#InfoText').text(json.msg)
                            $('#FaError').show()
                            $('#FaWait').hide()
                            $('#timeText').hide()
                        }
                        else {
                            $('#InfoText').text('Упс! Кажется что-то пошло не так')
                            $('#FaError').show()
                            $('#FaWait').hide()
                            $('#timeText').hide()
                        }
                    }).fail(function(){ 
                        $('#InfoText').text('Упс! Кажется что-то пошло не так')
                        $('#FaError').show()
                        $('#FaWait').hide()
                        $('#timeText').hide()
                    });
                })

	            Hub.start().then(function () {
		            Hub.invoke('GetValidCookie', '{isp:IP}', '{isp:HourCacheToUser}', '{isp:HashToSignalR}');
	            }).catch(function(error) 
                {
                    $('#InfoText').text('Упс! Не удалось подключится к SignalR')
                    $('#FaError').show()
                    $('#FaWait').hide()
                    $('#timeText').hide()
                })

            }, {isp:WaitUser} );
        }
    </script>

{isp:AddCodeToHtml}

</body>
</html>