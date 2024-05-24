from django.shortcuts import render
from django.http import HttpResponse, JsonResponse
from django.views.decorators.csrf import csrf_exempt
import os
import numpy as np
from .controllers.orb_detector import load_reference_image, process_uploaded_image

# Chargement des images de référence
try:
    input_image, aug_image, input_keypoints, input_descriptors = load_reference_image()
except FileNotFoundError as e:
    raise RuntimeError(f"Error loading reference images: {e}")

@csrf_exempt
def handle_image_upload(request):
    if request.method == 'POST' and request.FILES.get('image'):
        image_file = request.FILES['image']
        nparr = np.frombuffer(image_file.read(), np.uint8)
        
        # Traitement de l'image téléchargée
        try:
            marker_coordinates = process_uploaded_image(nparr, input_descriptors, input_keypoints)
        except FileNotFoundError as e:
            return JsonResponse({'message': str(e)}, status=400)
        
        if marker_coordinates:
            # Convertir marker_coordinates en une liste de tuples
            marker_coordinates_list = [(float(x), float(y)) for x, y in marker_coordinates]
            return JsonResponse({'message': str(marker_coordinates_list)})
        else:
            return JsonResponse({'message': 'No marker detected'})
    else:
        return JsonResponse({'message': 'Image upload failed'}, status=400)


def index(request):
    return HttpResponse("<h1>Hello, world. You're at the polls index.</h1>")
