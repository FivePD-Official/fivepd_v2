import { Component, OnInit } from '@angular/core';
import {
  CRS,
  extend,
  icon as LIcon,
  latLng,
  latLngBounds,
  layerGroup,
  map,
  Map as LMap,
  Marker,
  Projection,
  tileLayer,
  Transformation
} from 'leaflet';

@Component({
  selector: 'app-dispatch',
  templateUrl: './dispatch.component.html',
  styleUrls: ['./dispatch.component.sass']
})
export class DispatchComponent implements OnInit {

  readonly center_x = 117.3;
  readonly center_y = 172.8;
  readonly scale_x = 0.02072;
  readonly scale_y = 0.0205;
  readonly AtlasStyle = tileLayer('/assets/atlas/{z}/tile_{x}_{y}.jpg', {
    minZoom: 2,
    maxZoom: 5,
    id: 'styleAtlas map',
    keepBuffer: 100,
  });

  readonly ExampleGroup = layerGroup();

  readonly Icons = {
    "Example": this.ExampleGroup,
  };

  mymap!: LMap;
  CUSTOM_CRS: any;
  marker: any;
  azurirajmarker: any;

  // layersControl = L.control.layers({ "Satelite": SateliteStyle,"Atlas": AtlasStyle,"Grid":GridStyle}).addTo(mymap);

  ngOnInit(): void {
    this.CUSTOM_CRS = extend({}, CRS.Simple, {
      projection: Projection.LonLat,
      scale: function (zoom: any) {
        return Math.pow(2, zoom);
      },
      zoom: function (sc: any) {
        return Math.log(sc) / 0.6931471805599453;
      },
      distance: function (pos1: any, pos2: any) {
        var x_difference = pos2.lng - pos1.lng;
        var y_difference = pos2.lat - pos1.lat;
        return Math.sqrt(x_difference * x_difference + y_difference * y_difference);
      },
      transformation: new Transformation(this.scale_x, this.center_x, -this.scale_y, this.center_y),
      infinite: true,
    });

    const atlasMapSouthWest = latLng([-4034.146341463414, -5631.870330422804]);
    const atlasMapNorthEast = latLng([8380.487804878048, 6648.944755779789]);
    const atlasBounds = latLngBounds(atlasMapSouthWest, atlasMapNorthEast);
    this.mymap = map('map', {
      crs: this.CUSTOM_CRS,
      minZoom: 2,
      maxZoom: 5,
      preferCanvas: false,
      layers: [this.AtlasStyle, this.Icons["Example"]],
      center: [0, 0],
      zoom: 3,
      zoomControl: false,
    });
    this.mymap.setMaxBounds(atlasBounds);

    this.mymap.on('drag', () => {
      this.mymap.panInsideBounds(atlasBounds, {animate: false});
    });

    this.mymap.on('click', e => {
      if (this.marker) { // check
        this.mymap.removeLayer(this.marker);
      }
      const {lat, lng} = e.latlng;
      const zoom = 3;
      this.marker = new Marker([lat, lng]).addTo(this.Icons["Example"]).bindPopup("<b>X: " + lng.toFixed(3) + " | Y: " + lat.toFixed(3) + "</b>");
      this.mymap.setView([lat, lng], zoom);
      this.mymap.removeLayer(this.azurirajmarker);
    });
  }

  customIcon(icon: any) {
    return LIcon({
      iconUrl: `blips/${icon}.png`,
      iconSize: [30, 30],
      iconAnchor: [20, 20],
      popupAnchor: [-10, -27]
    });
  }

  // $("#formular").submit(function( event ) {
  //
  //   event.preventDefault();
  //   if ($.trim($("#xinput").val()) === "" || $.trim($("#yinput").val()) === "") {
  //     alert('Please fill X and Y coords');
  //     return false;
  //   }
  //
  //   mymap.eachLayer((layer) => {
  //     if(layer['_latlng']!=undefined)
  //       layer.remove();
  //   });
  //
  //
  //
  //
  //   var zoom = 16;
  //   var xpoz = $("#xinput").val();
  //   var ypoz = $("#yinput").val();
  //   azurirajmarker = new L.marker([ypoz,xpoz]).addTo(Icons["Example"]).bindPopup("<b>X: "+xpoz+" | Y: "+ypoz+"</b>");
  //   mymap.setView([ypoz, xpoz], zoom);
  //   $("#markerSkripta").empty().append("<script>"+azurirajmarker+"</script>");
  //
  // });


}
