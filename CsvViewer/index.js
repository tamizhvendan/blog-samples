(function(window){
  window.location.queryString = {};
  window.location.search.substr(1).split("&").forEach(function (pair) {
      if (pair === "") return;
      var parts = pair.split("=");
      location.queryString[parts[0]] = parts[1] &&
          decodeURIComponent(parts[1].replace(/\+/g, " "));
  });
})(window);

$(function(){

  if (location.queryString.msg) {
    $("#msg").show().text(location.queryString.msg)
  }
  $("#viewQueryData").on('click', function(e){
    e.preventDefault();
    $.getJSON('/view', {
      query : $("#query").val()
    }).done(function(data){
      if (data.length) {
        var headers = Object.keys(data[0]).map(function(name){
            return "<th>" + name + "</th>";
        }).join('');;
        var header = "<thead><tr>" + headers + "</tr><thead>";
        var bodyContent = data.map(function(o){
          var values = Object.keys(o).map(function(k){
            return "<td>" + o[k] + "</td>";
          }).join('');
          return "<tr>" + values + "</tr>";
        }).join('');
        var body = "<tbody>" + bodyContent + "</tbody>";
        var table = "<table class='table'>" + header + body + "</table>";
        $("#output").show().html(table);
      } else {
        $("#output").show().html("Zero rows found");
      }
    }).fail(function(err){

      $("#outputError").show().html("<pre>" + err.responseText + "</pre>");
    })
  });
});
