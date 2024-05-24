from django.shortcuts import render
from django.http import HttpResponse, JsonResponse
from django.views.decorators.csrf import csrf_exempt
import os
import numpy as np
import cv2
from .controllers.orb_detector import load_reference_image, process_uploaded_image
import uuid

@csrf_exempt
def handle_frames(request):
    if request.method == 'POST' and request.FILES.get('image'):
        image_file = request.FILES['image']
        nparr = np.frombuffer(image_file.read(), np.uint8)

        # Retrieve the data from the session
        input_image_list = request.session.get('input_image')
        input_keypoints_list = request.session.get('input_keypoints')
        input_descriptors_list = request.session.get('input_descriptors')

        if not input_image_list or not input_keypoints_list or not input_descriptors_list:
            return JsonResponse({'message': 'Reference image data not found'}, status=400)

        # Convert the lists back to their original formats
        input_image = np.array(input_image_list, dtype=np.uint8)
        input_keypoints = [
            cv2.KeyPoint(
                x=float(kp['pt'][0]), 
                y=float(kp['pt'][1]), 
                size=float(kp['size']),  # Corrected parameter name
                angle=float(kp['angle']), 
                response=float(kp['response']), 
                octave=int(kp['octave']), 
                class_id=int(kp['class_id'])
            ) for kp in input_keypoints_list
        ]
        input_descriptors = np.array(input_descriptors_list, dtype=np.uint8)  # Adjust dtype if necessary

        # Process the uploaded image using the loaded reference data
        try:
            marker_coordinates = process_uploaded_image(nparr, input_descriptors, input_keypoints)
        except FileNotFoundError as e:
            return JsonResponse({'message': str(e)}, status=400)

        if marker_coordinates:
            marker_coordinates_list = [(float(x), float(y)) for x, y in marker_coordinates]
            return JsonResponse({'message': str(marker_coordinates_list)})
        else:
            return JsonResponse({'message': 'No marker detected'})
    else:
        return JsonResponse({'message': 'Image upload failed'}, status=400)


@csrf_exempt
def handle_image_target(request):
    if request.method == 'POST' and request.FILES.get('image'):
        image_file = request.FILES['image']
        nparr = np.frombuffer(image_file.read(), np.uint8)

        try:
            input_image, input_keypoints, input_descriptors = load_reference_image(nparr)
        except FileNotFoundError as e:
            return JsonResponse({'message': str(e)}, status=400)

        # Store the data in the session
        request.session['input_image'] = input_image.tolist()  # Convert ndarray to list for serialization
        request.session['input_keypoints'] = [
            {
                'pt': kp.pt,
                'size': kp.size,
                'angle': kp.angle,
                'response': kp.response,
                'octave': kp.octave,
                'class_id': kp.class_id
            } for kp in input_keypoints
        ]
        request.session['input_descriptors'] = input_descriptors.tolist()  # Convert ndarray to list for serialization

        return JsonResponse({'message': 'Image uploaded successfully'})
    else:
        return JsonResponse({'message': 'Image upload failed'}, status=400)


def index(request):
    return HttpResponse("<h1>Hello, world. You're at the polls index.</h1>")
